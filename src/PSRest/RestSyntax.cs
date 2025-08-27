
namespace PSRest;

public interface IRestSyntax;

public class RestComment : IRestSyntax
{
    public required string Start { get; init; }
    public required string Text { get; init; }
}

public class RestVariable : IRestSyntax
{
    public required string Name { get; init; }
    public required string Value { get; init; }
}

public class RestRequest : IRestSyntax
{
    public RestRequest(IEnumerable<KeyValuePair<string, string>> headers)
    {
        Headers = new(headers, StringComparer.OrdinalIgnoreCase);
    }
    public required string Method { get; init; }
    public required string Url { get; init; }
    public required Version? Version { get; init; }
    public Dictionary<string, string> Headers { get; }
    public required string Body { get; init; }
}
