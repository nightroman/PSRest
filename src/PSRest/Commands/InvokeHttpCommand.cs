
using Sprache;
using System.Management.Automation;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

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

    readonly JsonSerializerOptions s_JsonSerializerOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
    };

    protected override void BeginProcessing()
    {
        RestEnvironment environment;
        string text;
        switch (ParameterSetName)
        {
            case PsnMain:
                var path = GetUnresolvedProviderPathFromPSPath(Path);
                environment = GetOrCreateEnvironment(System.IO.Path.GetDirectoryName(path)!);
                text = File.ReadAllText(path);
                break;
            case PsnText:
                environment = GetOrCreateEnvironment(SessionState.Path.CurrentFileSystemLocation.ProviderPath);
                text = Text!;
                break;
            default:
                throw new NotImplementedException();
        }

        var parsed = RestParser.Parser.TryParse(text);
        if (!parsed.WasSuccessful)
            throw new InvalidOperationException($"Parsing '{Path}: {parsed.Message}");

        var request = parsed.Value.FirstOrDefault(x => x is RestRequest) as RestRequest ??
            throw new InvalidOperationException($"Parsing '{Path}: HTTP request is not found.");

        // get variables first, new override old, then expand
        var variables = new Dictionary<string, string>();
        {
            foreach (var it in parsed.Value)
            {
                if (it is RestVariable var)
                    variables[var.Name] = var.Value;
            }

            foreach (var kv in variables)
                variables[kv.Key] = environment.ExpandVariables(kv.Value, variables);
        }

        var expBody = request.Body is null ? null : environment.ExpandVariables(request.Body, variables);
        if (request.Headers.FirstOrDefault(x => x.Key == "X-REQUEST-TYPE").Value == "GraphQL")
        {
            if (request.Method != "POST")
                throw new InvalidOperationException($"Expected 'POST' method for GraphQL requests, found '{request.Method}'");

            if (expBody is null)
                throw new InvalidOperationException("GraphQL request should have body.");

            var body = new
            {
                query = expBody,
            };

            expBody = JsonSerializer.Serialize(body);
        }

        var expUrl = environment.ExpandVariables(request.Url, variables);
        var message = new HttpRequestMessage(HttpMethod.Parse(request.Method), expUrl);

        foreach (var header in request.Headers)
        {
            var expValue = environment.ExpandVariables(header.Value, variables);
            message.Headers.TryAddWithoutValidation(header.Key, expValue);
        }

        if (expBody is { })
            message.Content = new StringContent(expBody, Encoding.UTF8, Const.MediaTypeJson);

        var client = new HttpClient();
        HttpResponseMessage response = client.Send(message);

        var contentString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(contentString);

        if (response.Content.Headers.ContentType?.MediaType == Const.MediaTypeJson)
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
