using Sprache;
using System.Management.Automation;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Xml;
using IOPath = System.IO.Path;

namespace PSRest.Commands;

[Cmdlet("Invoke", "RestHttp", DefaultParameterSetName = PsnMain)]
[OutputType(typeof(string))]
public sealed class InvokeHttpCommand : BaseEnvironmentCmdlet
{
    const string PsnMain = "Main";
    const string PsnText = "Text";

    [Parameter(Position = 0, Mandatory = true, ParameterSetName = PsnMain)]
    public string? Path { get; set; }

    [Parameter(Mandatory = true, ParameterSetName = PsnText)]
    public string? Text { get; set; }

    [Parameter]
    public string? HeadersVariable { get; set; }

    [Parameter]
    public string? Tag { get; set; } = System.Environment.GetEnvironmentVariable(Const.EnvVarTag);

    static readonly JsonSerializerOptions s_JsonSerializerOptions = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, WriteIndented = true };

    string _location = null!;
    RestEnvironment _environment = null!;
    Dictionary<string, string> _variables = null!;

    string BodyToContent(string body)
    {
        //! mind xml
        if (!body.StartsWith("< ") && !body.StartsWith("<@ "))
            return body;

        bool expand = body.StartsWith("<@");
        var path = IOPath.Combine(_location, body[(expand ? 3 : 2)..].Trim());

        body = File.ReadAllText(path).Trim();
        return expand ? _environment.ExpandVariables(body, _variables) : body;
    }

    string BodyToGraphQL(string body, string? expOperationName)
    {
        string[] parts = Regexes.EmptyLine().Split(body, 2);

        // query
        var query = BodyToContent(parts[0]);
        var dto = new Dictionary<string, object?>(3) { { "query", query } };

        // variables
        if (parts.Length > 1)
            dto.Add("variables", JsonSerializer.Deserialize<object>(parts[1]));

        // operationName
        if (expOperationName is { })
            dto.Add("operationName", expOperationName);

        return JsonSerializer.Serialize(dto);
    }

    static string FormatXml(string xml)
    {
        var doc = new XmlDocument();
        doc.LoadXml(xml);

        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            NewLineChars = "\n",
            NewLineHandling = NewLineHandling.Replace,
            OmitXmlDeclaration = true,
        };

        using var sw = new StringWriter();
        using var writer = XmlWriter.Create(sw, settings);
        doc.Save(writer);

        return sw.ToString();
    }

    RestRequest? FindRequest(IEnumerable<AnySyntax> source)
    {
        if (!string.IsNullOrEmpty(Tag))
        {
            //! assume list
            var list = (List<AnySyntax>)source;

            // find the comment ###, by line number or content
            int iComment;
            if (int.TryParse(Tag, out var lineNumber))
            {
                iComment = list.FindLastIndex(x => x is RestComment c && c.IsSeparator && c.Pos?.Line <= lineNumber);
            }
            else
            {
                iComment = list.FindLastIndex(x => x is RestComment c && c.IsSeparator && c.Text[2..].Trim() == Tag);
            }

            // take the first request after ###
            if (iComment >= 0)
            {
                for (int i = iComment + 1; i < list.Count; i++)
                {
                    if (list[i] is RestRequest r)
                        return r;
                }
            }
        }
        return source.OfType<RestRequest>().FirstOrDefault();
    }

    static List<RestComment> GetPromptsAndNamed(List<AnySyntax> source, RestRequest request, out string? named)
    {
        var r = new List<RestComment>();
        named = null;

        int index = source.IndexOf(request);
        for (int i = index; --i >= 0;)
        {
            if (source[i] is RestComment comment)
            {
                if (comment.IsSeparator)
                    break;

                if (comment.Prompt is { })
                {
                    r.Add(comment);
                }
                else if (comment.Named is { } str && named is null)
                {
                    named = str;
                }
            }
        }

        r.Reverse();
        return r;
    }

    protected override void MyBeginProcessing()
    {
        string text;
        switch (ParameterSetName)
        {
            case PsnMain:
                Path = GetUnresolvedProviderPathFromPSPath(Path);
                _location = IOPath.GetDirectoryName(Path)!;
                text = File.ReadAllText(Path);
                break;
            case PsnText:
                _location = SessionState.Path.CurrentFileSystemLocation.ProviderPath;
                text = Text!;
                break;
            default:
                throw new NotImplementedException();
        }
        _environment = GetOrCreateEnvironment(_location);

        var parsed = RestParser.Parser.TryParse(text);
        if (!parsed.WasSuccessful)
            throw new FormatException($"Parsing '{Path}: {parsed.Message}");

        // select tagged request
        var syntaxes = parsed.Value.ToList();
        var request = FindRequest(syntaxes) ??
            throw new FormatException($"Parsing '{Path}: HTTP request is not found.");

        // variables and prompts
        string? named = null;
        _variables = [];
        {
            // get raw vars, new replace old
            foreach (var it in parsed.Value)
            {
                if (it is RestVariable var)
                    _variables[var.Name] = var.Value;
            }

            // then expand, vars may use other vars
            foreach (var kv in _variables)
                _variables[kv.Key] = _environment.ExpandVariables(kv.Value, _variables);

            // then get prompt vars, replace old
            var comments = GetPromptsAndNamed(syntaxes, request, out named);
            foreach (var comment in comments)
            {
                var name = comment.Prompt!;
                var prompt = comment.Text.Length > 0 ? comment.Text : name;
                var maskInput = "|password|Password|PASSWORD|passwd|Passwd|PASSWD|pass|Pass|PASS|".Contains($"|{name}|");
                var res = ScriptBlock.Create("Read-Host -Prompt $args[0] -MaskInput:$args[1]").Invoke(prompt, maskInput);
                _variables[name] = res.Count > 0 ? res[0].ToString() : string.Empty;
            }
        }

        // body
        var expBody = request.Body is null ? null : _environment.ExpandVariables(request.Body, _variables);

        // GraphQL or content body
        if (request.Headers.Find(x
            => x.Key.Equals(Const.XRequestType, StringComparison.OrdinalIgnoreCase)
            && x.Value.Equals("GraphQL", StringComparison.OrdinalIgnoreCase)) is { } headerRequestType)
        {
            //! as REST Client
            request.Headers.Remove(headerRequestType);

            if (request.Operation.Method != "POST")
                throw new FormatException($"Expected 'POST' method for GraphQL requests, found '{request.Operation.Method}'");

            if (expBody is null)
                throw new FormatException("GraphQL request should have body.");

            string? expOperationName = null;
            if (request.Headers.Find(x
                => x.Key.Equals(Const.XGraphQLOperation, StringComparison.OrdinalIgnoreCase)) is { } headerGraphQLOperation)
            {
                request.Headers.Remove(headerGraphQLOperation);
                expOperationName = _environment.ExpandVariables(headerGraphQLOperation.Value, _variables);
            }

            expBody = BodyToGraphQL(expBody, expOperationName);
        }
        else if (expBody is { })
        {
            expBody = BodyToContent(expBody);
        }

        // HTTP request message with URL
        var expUrl = _environment.ExpandVariables(request.Operation.Url, _variables);
        var message = new HttpRequestMessage(HttpMethod.Parse(request.Operation.Method), expUrl);

        // HTTP version
        if (request.Operation.Version != default)
            message.Version = request.Operation.Version;

        // HTTP headers
        foreach (var header in request.Headers)
        {
            var expValue = _environment.ExpandVariables(header.Value, _variables);
            message.Headers.TryAddWithoutValidation(header.Key, expValue);
        }

        // HTTP content
        if (expBody is { })
            message.Content = new StringContent(expBody, Encoding.UTF8, Const.MediaTypeJson);

        // HTTP client, send
        WriteProgress(new(1, Const.MyName, "Sending HTTP request..."));
        var client = new HttpClient();
        HttpResponseMessage response = client.Send(message);
        WriteProgress(new(0, Const.MyName, "Done"));

        // variables
        if (HeadersVariable is { })
            SessionState.PSVariable.Set(new(HeadersVariable, response.Content.Headers));

        // content
        var contentString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(contentString);

        // JSON, format
        if (response.Content.Headers.ContentType?.MediaType?.Contains("json", StringComparison.OrdinalIgnoreCase) == true)
        {
            try
            {
                contentString = JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(contentString), s_JsonSerializerOptions);
            }
            catch (Exception ex)
            {
                WriteWarning($"Formatting JSON: {ex.Message}");
            }
        }
        else if (response.Content.Headers.ContentType?.MediaType?.Contains(Const.MediaTypeXml, StringComparison.OrdinalIgnoreCase) == true)
        {
            try
            {
                contentString = FormatXml(contentString);
            }
            catch (Exception ex)
            {
                WriteWarning($"Formatting XML: {ex.Message}");
            }
        }

        WriteObject(contentString);

        if (named is { })
        {
            RestEnvironment.SetNamedData(named, new(expBody ?? string.Empty, request.Headers, contentString, response.Content.Headers));
        }
    }
}
