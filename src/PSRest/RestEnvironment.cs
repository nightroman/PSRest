using DotNetEnv;
using DotNetEnv.Extensions;
using Json.Path;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;

namespace PSRest;

public class RestEnvironment
{
    private readonly string _Name;
    private readonly string _Path;
    private readonly string? _DotEnvFile;
    private readonly string? _SettingsFile;

    private static readonly Dictionary<string, NamedData> s_namedData = [];

    private Dictionary<string, string>? _dataEnvCurrent;
    private Dictionary<string, string>? _dataEnvShared;
    private Dictionary<string, string>? _dataDotEnv;

    public record Args(
        string Path,
        string? Name = null,
        string? DotEnvFile = null,
        string? SettingsFile = null);

    public record NamedData(
        string RequestBody,
        List<RestHeader> RequestHeaders,
        string ResponseBody,
        HttpContentHeaders ResponseHeaders);

    public RestEnvironment(Args args)
    {
        _Name =
            args.Name is { } name1 && name1.Length > 0 ? name1 :
            Environment.GetEnvironmentVariable(Const.EnvVarDefault) is { } name2 && name2.Length > 0 ? name2 :
            Const.EnvNameShared;

        _Path = args.Path;
        _DotEnvFile = args.DotEnvFile;
        _SettingsFile = args.SettingsFile;
    }

    public static void SetNamedData(string name, NamedData data)
    {
        s_namedData[name] = data;
    }

    public string ExpandVariables(string value, Dictionary<string, string>? vars = null)
    {
        var mm = Regexes.RestVariable().Matches(value);
        for (int i = mm.Count; --i >= 0;)
        {
            var m = mm[i];
            var var = GetVariable(m.Groups[1].Value, VariableType.Any, vars);
            value = value[0..m.Index] + var + value[(m.Index + m.Length)..];
        }
        return value;
    }

    public string GetVariable(string name, VariableType type, Dictionary<string, string>? vars = null)
    {
        if ((name = name.Trim()).Length == 0)
            throw new InvalidOperationException("Not supported empty variable '{{}}'.");

        if (type == VariableType.Any)
        {
            if (name.StartsWith('$'))
            {
                var system = GetSystemVariable(name);
                if (system is { })
                    return system;

                var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                type = parts[0] switch
                {
                    Const.VarTypeShared => VariableType.Shared,
                    Const.VarTypeDotEnv => VariableType.DotEnv,
                    Const.VarTypeProcessEnv => VariableType.ProcessEnv,
                    _ => throw new ArgumentException($"Unknown variable type '{parts[0]}' in '{name}'.")
                };

                if (parts.Length != 2)
                    throw new ArgumentException($"Unknown variable '{name}'.");

                name = parts[1];
            }
            else if (name.Contains('.'))
            {
                return GetRequestVariable(name);
            }
            else
            {
                type = VariableType.Env;
            }
        }

        if (name.StartsWith('%'))
        {
            var name2 = name[1..];
            name = GetEnvVariable(name2) ??
                throw new InvalidOperationException($"Cannot find variable '{name2}'.");
        }

        return type switch
        {
            VariableType.Env => GetEnvVariable(name, false, vars),
            VariableType.Shared => GetEnvVariable(name, true),
            VariableType.DotEnv => GetDotEnvVariable(name),
            VariableType.ProcessEnv => GetProcessEnvVariable(name),
            _ => throw new NotSupportedException()
        };
    }

    private static string? GetSystemVariable(string name)
    {
        if (name == Const.SystemGuid)
        {
            return Guid.NewGuid().ToString();
        }

        if (name.StartsWith(Const.SystemRandomInt))
        {
            var span = name.AsSpan(Const.SystemRandomInt.Length).Trim();
            int index = span.IndexOf(' ');
            if (index < 0 || !int.TryParse(span[0..index], out int min) || !int.TryParse(span[(index + 1)..], out int max))
                throw new FormatException($"Parsing '{name}': expected '{Const.SystemRandomInt} min max'.");

            return Random.Shared.Next(min, max).ToString();
        }

        if (name.StartsWith(Const.SystemTimestamp))
        {
            if (name == Const.SystemTimestamp)
                return DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            var offset = TextKit.ParseOffset(name, name.AsSpan(Const.SystemTimestamp.Length));
            var timestamp = DateTimeOffset.UtcNow + offset;
            return timestamp.ToUnixTimeSeconds().ToString();
        }

        if (name.StartsWith(Const.SystemDatetime))
        {
            return TextKit.ParseDatetime(name, Const.SystemDatetime, DateTime.UtcNow);
        }

        if (name.StartsWith(Const.SystemLocalDatetime))
        {
            return TextKit.ParseDatetime(name, Const.SystemLocalDatetime, DateTime.Now);
        }

        return null;
    }

    private static string GetProcessEnvVariable(string name)
    {
        //! as REST Client
        return Environment.GetEnvironmentVariable(name) ?? string.Empty;
    }

    private string GetDotEnvVariable(string name)
    {
        if (_dataDotEnv is null)
        {
            var path = _DotEnvFile ?? Path.Join(_Path, Const.DotEnvFile);
            try
            {
                _dataDotEnv = Env.NoEnvVars().Load(path).ToDotEnvDictionary(CreateDictionaryOption.TakeLast);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Loading '{path}': {ex.Message}", ex);
            }
        }

        if (_dataDotEnv.TryGetValue(name, out var value))
            return value;

        throw new InvalidOperationException($".env variable '{name}' is undefined.");
    }

    private static string? GetSettingsPath(string dir)
    {
        for (; ; )
        {
            var path = Path.Join(dir, Const.VSCodeDir, Const.SettingsFile);
            if (File.Exists(path))
                return path;

            dir = Path.GetDirectoryName(dir)!;
            if (string.IsNullOrEmpty(dir))
                return null;
        }
    }

    private void InitEnv()
    {
        var path = _SettingsFile ?? GetSettingsPath(_Path) ??
            throw new InvalidOperationException("Cannot find '.vscode/settings.json'.");

        try
        {
            JsonDocumentOptions options = new() { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip };
            using var stream = File.OpenRead(path);
            using var doc = JsonDocument.Parse(stream, options);

            if (!doc.RootElement.TryGetProperty(Const.SettingsKeyRoot, out var root))
                throw new InvalidOperationException($"Cannot find '{Const.SettingsKeyRoot}'.");

            _dataEnvShared = [];
            if (root.TryGetProperty(Const.EnvNameShared, out var shared))
            {
                foreach (var it in shared.EnumerateObject())
                    _dataEnvShared.Add(it.Name, it.Value.ToString());
            }

            if (_Name == Const.EnvNameShared)
                return;

            if (!root.TryGetProperty(_Name, out var current))
                throw new InvalidOperationException($"Cannot find environment '{_Name}'.");

            _dataEnvCurrent = [];
            foreach (var it in current.EnumerateObject())
                _dataEnvCurrent.Add(it.Name, it.Value.ToString());
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Loading '{path}': {ex.Message}", ex);
        }
    }

    private string GetEnvVariable(string name, bool shared = false, Dictionary<string, string>? vars = null)
    {
        if (!shared && vars is { } && vars.TryGetValue(name, out var value))
            return value;

        if (_dataEnvShared is null)
            InitEnv();

        if (!shared && _dataEnvCurrent is { } && _dataEnvCurrent.TryGetValue(name, out value))
            return ExpandVariables(value);

        if (_dataEnvShared!.TryGetValue(name, out value))
            return value;

        throw new InvalidOperationException($"Variable '{name}' is undefined.");
    }

    private static string GetRequestVariable(string name)
    {
        InvalidOperationException NotSupported() => new($"Not supported request variable: '{name}'.");

        var parts = name.Split('.', 4);
        if (parts.Length < 4)
            throw NotSupported();

        if (!s_namedData.TryGetValue(parts[0], out var data))
            throw new InvalidOperationException($"Invoke required request named '{parts[0]}'.");

        switch (parts[1])
        {
            case "request":
                switch (parts[2])
                {
                    case "body":
                        if (parts[3] == "*")
                            return data.RequestBody;

                        if (data.RequestHeaders.Any(x =>
                            x.Key.Equals(Const.MediaContentType, StringComparison.OrdinalIgnoreCase) &&
                            x.Value.Contains("/json")))
                            return EvalBodyJsonPath(data.RequestBody, parts[3]);

                        if (data.RequestHeaders.Any(x =>
                            x.Key.Equals(Const.MediaContentType, StringComparison.OrdinalIgnoreCase) &&
                            x.Value.Contains("/xml")))
                            return EvalBodyXPath(data.RequestBody, parts[3]);

                        throw NotSupported();

                    case "headers":
                        var key = parts[3];
                        var values = data.RequestHeaders
                            .Where(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                            .Select(x => x.Value);
                        return string.Join(",", values);
                }
                break;

            case "response":
                switch (parts[2])
                {
                    case "body":
                        if (parts[3] == "*")
                            return data.ResponseBody;

                        if (data.ResponseHeaders.ContentType?.MediaType?.Contains("/json") == true)
                            return EvalBodyJsonPath(data.ResponseBody, parts[3]);

                        if (data.ResponseHeaders.ContentType?.MediaType?.Contains("/xml") == true)
                            return EvalBodyXPath(data.ResponseBody, parts[3]);

                        throw NotSupported();

                    case "headers":
                        var key = parts[3];
                        return data.ResponseHeaders.TryGetValues(key, out var values) ? string.Join(",", values) : string.Empty;
                }
                break;
        }

        throw NotSupported();
    }

    private static string EvalBodyXPath(string body, string path)
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(body);

        var node = xmlDoc.SelectSingleNode(path);
        if (node is null)
            return string.Empty; //! unlike REST Client variable name

        if (node.HasChildNodes)
            return node.InnerXml;

        return node.InnerText;
    }

    private static string EvalBodyJsonPath(string body, string path)
    {
        var select = JsonPath.Parse(path);
        if (select.Segments.Length == 0)
            throw new InvalidOperationException($"JSON path should have selectors: '{path}'.");

        var options = new JsonDocumentOptions { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip };
        JsonNode? root = JsonNode.Parse(body, null, options);

		//! select and cache, avoid 2+ execution
		var nodes = select.Evaluate(root).Matches.ToList();
        if (nodes.Count == 0)
            return string.Empty;

        if (nodes.Count >= 2)
            throw new InvalidOperationException($"JSON path should be single node: '{path}'.");

        var node = nodes[0];
        if (node.Value is null)
            return string.Empty;

        return node.Value.ToJsonString();
    }
}
