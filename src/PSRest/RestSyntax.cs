using Sprache;

namespace PSRest;

public abstract class IRestSyntax
{
    public Position? Pos { get; private set; }
    protected void MySetPos(Position startPos)
    {
        Pos = startPos;
    }
}

public class RestComment : IRestSyntax, IPositionAware<RestComment>
{
    public RestComment SetPos(Position startPos, int length) { MySetPos(startPos); return this; }
    public required string Start { get; init; }
    public required string Text { get; init; }
}

public class RestVariable : IRestSyntax
{
    public required string Name { get; init; }
    public required string Value { get; init; }
}

public class RestRequestLine : IRestSyntax, IPositionAware<RestRequestLine>
{
    public RestRequestLine SetPos(Position startPos, int length) { MySetPos(startPos); return this; }
    public required string Method { get; init; }
    public required string Url { get; init; }
    public required Version Version { get; init; }
}

public class RestRequest : IRestSyntax
{
    public RestRequest(IEnumerable<KeyValuePair<string, string>> headers)
    {
        Headers = new(headers, StringComparer.OrdinalIgnoreCase);
    }
    public required RestRequestLine Line { get; init; }
    public Dictionary<string, string> Headers { get; }
    public required string Body { get; init; }
}
