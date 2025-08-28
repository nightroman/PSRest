
using Sprache;
using System.Management.Automation;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
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

    static readonly JsonSerializerOptions s_JsonSerializerOptions = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, WriteIndented = true };

    string _location = null!;
    RestEnvironment _environment = null!;
    Dictionary<string, string> _variables = null!;

    string BodyToContent(string body)
    {
        if (!body.StartsWith('<'))
            return body;

        bool expand = body.StartsWith("<@");
        var path = IOPath.Combine(_location, body[(expand ? 2 : 1)..].Trim());

        body = File.ReadAllText(path).Trim();
        return expand ? _environment.ExpandVariables(body, _variables) : body;
    }

    string BodyToGraphQL(string body)
    {
        string[] parts = Regexes.EmptyLine().Split(body, 2);

        var query = BodyToContent(parts[0]);

        object? variables = null;
        if (parts.Length > 1)
            variables = JsonSerializer.Deserialize<object>(parts[1]);

        return JsonSerializer.Serialize(new
        {
            query,
            variables
        });
    }

    protected override void BeginProcessing()
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

        var request = parsed.Value.FirstOrDefault(x => x is RestRequest) as RestRequest ??
            throw new FormatException($"Parsing '{Path}: HTTP request is not found.");

        // get variables first, new override old, then expand
        _variables = [];
        {
            foreach (var it in parsed.Value)
            {
                if (it is RestVariable var)
                    _variables[var.Name] = var.Value;
            }

            foreach (var kv in _variables)
                _variables[kv.Key] = _environment.ExpandVariables(kv.Value, _variables);
        }

        // body
        var expBody = request.Body is null ? null : _environment.ExpandVariables(request.Body, _variables);

        // GraphQL or content body
        if (request.Headers.TryGetValue(Const.XRequestType, out var requestType) && requestType.Equals("GraphQL", StringComparison.OrdinalIgnoreCase))
        {
            //! as REST Client
            request.Headers.Remove(Const.XRequestType);

            if (request.Method != "POST")
                throw new FormatException($"Expected 'POST' method for GraphQL requests, found '{request.Method}'");

            if (expBody is null)
                throw new FormatException("GraphQL request should have body.");

            expBody = BodyToGraphQL(expBody);
        }
        else if (expBody is { })
        {
            expBody = BodyToContent(expBody);
        }

        // HTTP request message with URL
        var expUrl = _environment.ExpandVariables(request.Url, _variables);
        var message = new HttpRequestMessage(HttpMethod.Parse(request.Method), expUrl);

        // HTTP version
        if (request.Version != default)
            message.Version = request.Version;

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
        var client = new HttpClient();
        HttpResponseMessage response = client.Send(message);

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

        WriteObject(contentString);
    }
}
