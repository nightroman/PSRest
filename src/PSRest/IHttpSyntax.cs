
namespace PSRest;

public interface IHttpSyntax;

public class HttpComment : IHttpSyntax
{
    public required string Start { get; init; }
    public required string Text { get; init; }
}

public class HttpVariable : IHttpSyntax
{
    public required string Name { get; init; }
    public required string Value { get; init; }
}

public class HttpRequest : IHttpSyntax
{
    public required string Method { get; init; }
    public required string Url { get; init; }
    public required Dictionary<string, string> Headers { get; init; }
    public required string Body { get; init; }
}
