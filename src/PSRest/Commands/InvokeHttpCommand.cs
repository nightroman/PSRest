using Json.Path;
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
    private const string PsnMain = "Main";
    private const string PsnText = "Text";

    [Parameter(Position = 0, Mandatory = true, ParameterSetName = PsnMain)]
    public string? Path { get; set; }

    [Parameter(Mandatory = true, ParameterSetName = PsnText)]
    public string? Text { get; set; }

    [Parameter]
    public string? HeadersVariable { get; set; }

    [Parameter]
    public string? Tag { get; set; } = System.Environment.GetEnvironmentVariable(Const.EnvVarTag);

    private static readonly JsonSerializerOptions s_JsonSerializerOptions =
        new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, WriteIndented = true };

    // nested shared data
    private Stack<string> _calls = null!;
    private string _location = null!;
    private RestEnvironment _environment = null!;
    private List<AnySyntax> _syntaxes = null!;

    private string BodyToContent(string body, VariableArgs args)
    {
        //! mind xml
        if (!body.StartsWith("< ") && !body.StartsWith("<@ "))
            return body;

        bool expand = body.StartsWith("<@");
        var path = IOPath.Combine(_location, body[(expand ? 3 : 2)..].Trim());

        body = File.ReadAllText(path).Trim();
        return expand ? _environment.ExpandVariables(body, args) : body;
    }

    private string BodyToGraphQL(string body, string? expOperationName, VariableArgs args)
    {
        string[] parts = Regexes.EmptyLine().Split(body, 2);

        // query
        var query = BodyToContent(parts[0], args);
        var dto = new Dictionary<string, object?>(3) { { "query", query } };

        // variables
        if (parts.Length > 1)
            dto.Add("variables", JsonSerializer.Deserialize<object>(parts[1]));

        // operationName
        if (expOperationName is { })
            dto.Add("operationName", expOperationName);

        return JsonSerializer.Serialize(dto);
    }

    private static string FormatXml(string xml)
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

    private RestRequest? FindTaggedRequest(IEnumerable<AnySyntax> source)
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
                if (iComment < 0)
                    throw new InvalidOperationException($"Cannot find request tag '{Tag}'.");
            }

            // take the first request after ### or -1
            for (int i = iComment + 1; i < list.Count; i++)
            {
                if (list[i] is RestRequest r)
                    return r;
            }
        }
        return source.OfType<RestRequest>().FirstOrDefault();
    }

    private RestRequest FindNamedRequest(IEnumerable<AnySyntax> source, string name)
    {
        //! assume list
        var list = (List<AnySyntax>)source;

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] is not RestComment comment || comment.IsSeparator || comment.Named is { } named && named != name)
                continue;

            while (++i < list.Count)
            {
                if (list[i] is RestRequest request)
                    return request;

                if (list[i] is RestComment comment2 && (comment2.IsSeparator || comment2.Named is { } named2 && named2 != name))
                    break;
            }
        }

        throw new InvalidOperationException($"Named request '{name}' is not found.");
    }

    private static List<RestComment> GetPromptsAndNamed(List<AnySyntax> source, RestRequest request, string? named1, out string? named2)
    {
        var r = new List<RestComment>();
        named2 = named1;

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
                else if (named1 is null && comment.Named is { } str && named2 is null)
                {
                    named2 = str;
                }
            }
        }

        r.Reverse();
        return r;
    }

    private void InvokeNamedRequest(string name)
    {
        if (_calls.Contains(name))
            throw new InvalidOperationException($"Cyclic reference of named request '{name}'.");

        try
        {
            _calls.Push(name);
            MyBeginProcessing();
        }
        finally
        {
            _calls.Pop();
        }
    }

    protected override void MyBeginProcessing()
    {
        // root call? init shared data for nested calls
        if (_calls is null)
        {
            _calls = new();

            // location, text, environment
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

            // parsed syntaxes
            var parsed = RestParser.Parser.TryParse(text);
            if (!parsed.WasSuccessful)
                throw new InvalidOperationException($"Parsing HTTP: {parsed.Message}");
            _syntaxes = [.. parsed.Value];
        }

        // select tagged or named request
        RestRequest request;
        string? named = null;
        if (_calls.Count == 0)
        {
            request = FindTaggedRequest(_syntaxes) ??
                throw new InvalidOperationException("HTTP request is not found.");
        }
        else
        {
            named = _calls.Peek();
            request = FindNamedRequest(_syntaxes, named);
        }

        // variables and prompts
        var args = new VariableArgs { HttpFile = Path, Vars = [], InvokeNamedRequest = InvokeNamedRequest }!;
        {
            // get raw vars, new replace old
            foreach (var it in _syntaxes)
            {
                if (it is RestVariable var)
                    args.Vars[var.Name] = var.Value;
            }

            // then expand, vars may use other vars
            foreach (var kv in args.Vars)
                args.Vars[kv.Key] = _environment.ExpandVariables(kv.Value, args);

            // then get prompt vars, replace old
            var comments = GetPromptsAndNamed(_syntaxes, request, named, out named);
            foreach (var comment in comments)
            {
                var name = comment.Prompt!;
                var prompt = comment.Text.Length > 0 ? comment.Text : name;
                var maskInput = "|password|Password|PASSWORD|passwd|Passwd|PASSWD|pass|Pass|PASS|".Contains($"|{name}|");
                var res = ScriptBlock.Create("Read-Host -Prompt $args[0] -MaskInput:$args[1]").Invoke(prompt, maskInput);
                args.Vars[name] = res.Count > 0 ? res[0].ToString() : string.Empty;
            }
        }

        // body
        var expBody = request.Body is null ? null : _environment.ExpandVariables(request.Body, args);

        // GraphQL or content body
        if (request.Headers.Find(x
            => x.Key.Equals(Const.XRequestType, StringComparison.OrdinalIgnoreCase)
            && x.Value.Equals("GraphQL", StringComparison.OrdinalIgnoreCase)) is { } headerRequestType)
        {
            //! as REST Client
            request.Headers.Remove(headerRequestType);

            if (request.Operation.Method != "POST")
                throw new InvalidOperationException($"Expected 'POST' method for GraphQL requests, found '{request.Operation.Method}'");

            if (expBody is null)
                throw new InvalidOperationException("GraphQL request should have source code.");

            string? expOperationName = null;
            if (request.Headers.Find(x
                => x.Key.Equals(Const.XGraphQLOperation, StringComparison.OrdinalIgnoreCase)) is { } headerGraphQLOperation)
            {
                request.Headers.Remove(headerGraphQLOperation);
                expOperationName = _environment.ExpandVariables(headerGraphQLOperation.Value, args);
            }

            expBody = BodyToGraphQL(expBody, expOperationName, args);
        }
        else if (expBody is { })
        {
            expBody = BodyToContent(expBody, args);
        }

        // HTTP request message with URL
        var expUrl = _environment.ExpandVariables(request.Operation.Url, args);
        var message = new HttpRequestMessage(HttpMethod.Parse(request.Operation.Method), expUrl);

        // HTTP version
        if (request.Operation.Version != default)
            message.Version = request.Operation.Version;

        // HTTP headers
        foreach (var header in request.Headers)
        {
            var expValue = _environment.ExpandVariables(header.Value, args);
            message.Headers.TryAddWithoutValidation(header.Key, expValue);
        }

        // HTTP content
        if (expBody is { })
            message.Content = new StringContent(expBody, Encoding.UTF8, Const.MediaTypeJson);

        // HTTP client, send
        var client = new HttpClient();
        HttpResponseMessage response = client.Send(message);

        // out variables
        if (_calls.Count == 0)
        {
            if (HeadersVariable is { })
                SessionState.PSVariable.Set(new(HeadersVariable, response.Content.Headers));
        }

        // lazy content
        Lazy<string> contentString = new(() => response.Content.ReadAsStringAsync().GetAwaiter().GetResult());

        // set named data when file is used
        if (named is { })
        {
            RestEnvironment.SetNamedData(named, Path, new(expBody ?? string.Empty, request.Headers, contentString.Value, response.Content.Headers));
        }

        // fail?
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(contentString.Value);

        // write content

        if (_calls.Count > 0)
            return;

        string resultString;
        if (response.Content.Headers.ContentType?.MediaType?.Contains("/json", StringComparison.OrdinalIgnoreCase) == true)
        {
            // format JSON
            try
            {
                resultString = JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(contentString.Value), s_JsonSerializerOptions);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Formatting JSON: {ex.Message}");
            }
        }
        else if (response.Content.Headers.ContentType?.MediaType?.Contains("/xml", StringComparison.OrdinalIgnoreCase) == true)
        {
            // format XML
            try
            {
                resultString = FormatXml(contentString.Value);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Formatting XML: {ex.Message}");
            }
        }
        else
        {
            resultString = contentString.Value;
        }

        WriteObject(resultString);
    }
}
