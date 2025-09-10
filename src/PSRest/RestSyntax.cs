namespace PSRest;

public record RestComment : AnySyntax
{
    public required string Start { get; init; }
    public required string Text { get; init; }
    public string? Named { get; init; }
    public string? Prompt { get; init; }
    public bool IsSeparator => Start == "#" && Text.StartsWith("##");
}

public record RestVariable : AnySyntax
{
    public required string Name { get; init; }
    public required string Value { get; init; }
}

public record RestOperation : AnySyntax
{
    public required string Method { get; init; }
    public required string Url { get; init; }
    public required Version Version { get; init; }
}

public record RestHeader : AnySyntax
{
    public required string Key { get; init; }
    public required string Value { get; init; }
}

public record RestRequest : AnySyntax
{
    public required RestOperation Operation { get; init; }
    public required List<RestHeader> Headers { get; init; }
    public required string Body { get; init; }
}
