
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
    public required string Method { get; init; }
    public required string Url { get; init; }
    public required Dictionary<string, string> Headers { get; init; }
    public required string Body { get; init; }
}
