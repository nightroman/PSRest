
using DotNetEnv;
using DotNetEnv.Extensions;
using System.Text.Json;

namespace PSRest;

public class RestEnvironment
{
    readonly string _Name;
    readonly string _Path;
    readonly string? _DotEnvFile;
    readonly string? _SettingsFile;

    Dictionary<string, string>? _dataEnvCurrent;
    Dictionary<string, string>? _dataEnvShared;
    Dictionary<string, string>? _dataDotEnv;

    public record Args(
        string Path,
        string? Name = null,
        string? DotEnvFile = null,
        string? SettingsFile = null);

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

    public string? GetVariable(string name, VariableType type, Dictionary<string, string>? vars = null)
    {
        if (type == VariableType.Any)
        {
            if (name.StartsWith('$'))
            {
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
            _ => throw new NotSupportedException(),
        };
    }

    public static string? GetProcessEnvVariable(string name)
    {
        return Environment.GetEnvironmentVariable(name);
    }

    public string? GetDotEnvVariable(string name)
    {
        if (_dataDotEnv is null)
        {
            var path = _DotEnvFile ?? Path.Join(_Path, Const.DotEnvFile);
            try
            {
                _dataDotEnv = Env.NoEnvVars().Load(path).ToDotEnvDictionary(CreateDictionaryOption.TakeFirst);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Loading '{path}': {ex.Message}", ex);
            }
        }

        if (_dataDotEnv.TryGetValue(name, out var value))
            return value;

        return null;
    }

    static string? GetSettingsPath(string dir)
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

    void InitEnv()
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

    public string? GetEnvVariable(string name, bool shared = false, Dictionary<string, string>? vars = null)
    {
        if (!shared && vars is { } && vars.TryGetValue(name, out var value))
            return value;

        if (_dataEnvShared is null)
            InitEnv();

        if (!shared && _dataEnvCurrent is { } && _dataEnvCurrent.TryGetValue(name, out value))
            return ExpandVariables(value);

        if (_dataEnvShared!.TryGetValue(name, out value))
            return value;

        return null;
    }

    public string ExpandVariables(string value, Dictionary<string, string>? vars = null)
    {
        var mm = Regexes.RestVariable().Matches(value);
        for (int i = mm.Count; --i >= 0;)
        {
            var m = mm[i];
            var var = GetVariable(m.Groups[1].Value, VariableType.Any, vars);
            if (var is { })
                value = value[0..m.Index] + var + value[(m.Index + m.Length)..];
        }
        return value;
    }
}
