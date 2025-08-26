
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
    public string Path { get; set; } = null!;

    [Parameter(Mandatory = true, ParameterSetName = PsnText)]
    public string Text { get; set; } = null!;

    readonly JsonSerializerOptions s_JsonSerializerOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
    };

    protected override void BeginProcessing()
    {
        string text = ParameterSetName switch
        {
            PsnMain => File.ReadAllText(GetUnresolvedProviderPathFromPSPath(Path)),
            PsnText => Text,
            _ => throw new NotImplementedException()
        };

        text = Environment.ExpandVariables(text);

        var parsed = HttpParser.Parser.TryParse(text);
        if (!parsed.WasSuccessful)
            throw new InvalidOperationException($"Parsing '{Path}: {parsed.Message}");

        var request = parsed.Value.FirstOrDefault(x => x is HttpRequest) as HttpRequest ??
            throw new InvalidOperationException($"Parsing '{Path}: request is not found.");

        if (parsed.Value.FirstOrDefault(x => x is HttpVariable) is { })
            throw new NotSupportedException($"Parsing '{Path}: file variables are not yet supported.");

        var bodyString = request.Body;
        if (request.Headers.FirstOrDefault(x => x.Key == "X-REQUEST-TYPE").Value == "GraphQL")
        {
            if (request.Method != "POST")
                throw new InvalidOperationException($"Expected 'POST' method for GraphQL requests, found '{request.Method}'");

            if (bodyString is null)
                throw new InvalidOperationException("GraphQL request should have body.");

            var body = new
            {
                query = bodyString,
            };

            bodyString = JsonSerializer.Serialize(body);
        }

        var message = new HttpRequestMessage(HttpMethod.Parse(request.Method), request.Url);

        foreach (var header in request.Headers)
        {
            message.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (bodyString is { })
        {
            message.Content = new StringContent(bodyString, Encoding.UTF8, Const.MediaTypeJson);
        };

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
