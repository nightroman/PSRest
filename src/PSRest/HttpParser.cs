
using Sprache;

namespace PSRest;

public static class HttpParser
{
    public static readonly Parser<IHttpSyntax> WhiteSpaceParser =
        from _ in Parse.WhiteSpace.AtLeastOnce()
        select (IHttpSyntax)null!;

    public static readonly Parser<HttpComment> CommentParser =
        from leading in Parse.WhiteSpace.Many()
        from start in Parse.String("#").Or(Parse.String("//")).Text()
        from text in Parse.CharExcept("\r\n").Many().Text()
        from _ in Parse.LineTerminator
        select new HttpComment { Start = start, Text = text };

    public static readonly Parser<HttpVariable> VariableParser =
        from at in Parse.Char('@')
        from name in Parse.LetterOrDigit.AtLeastOnce().Text().Token()
        from eq in Parse.Char('=').Token()
        from value in Parse.CharExcept("\r\n").Many().Text()
        from _ in Parse.LineTerminator
        select new HttpVariable { Name = name, Value = value };

    public static readonly Parser<string> MethodParser =
        Parse.String("GET")
        .Or(Parse.String("POST"))
        .Or(Parse.String("PUT"))
        .Or(Parse.String("DELETE"))
        .Or(Parse.String("PATCH"))
        .Or(Parse.String("HEAD"))
        .Or(Parse.String("OPTIONS"))
        .Or(Parse.String("TRACE"))
        .Or(Parse.String("CONNECT"))
        .Text();

    public static readonly Parser<string> UrlParser =
        Parse.CharExcept("\r\n").Many().Text().Token();

    public static readonly Parser<(string, string)> RequestLineParser =
        from method in MethodParser
        from ws in Parse.WhiteSpace.AtLeastOnce()
        from url in UrlParser
        select (method, url);

    public static readonly Parser<KeyValuePair<string, string>> HeaderParser =
        from key in Parse.Char(c => !char.IsWhiteSpace(c) && c != ':', "key").Many().Text().Token()
        from colon in Parse.Char(':')
        from value in Parse.CharExcept("\r\n").Many().Text()
        from _ in Parse.LineTerminator
        select new KeyValuePair<string, string>(key, value.Trim());

    public static readonly Parser<string> BodyLineParser =
        from next in Parse.Not(Parse.String("###"))
        from line in Parse.CharExcept("\r\n").Many().Text()
        from _ in Parse.LineTerminator
        select line;

    public static readonly Parser<string> BodyParser =
        from blank in Parse.LineEnd.AtLeastOnce()
        from lines in BodyLineParser.Many()
        select string.Join('\n', lines);

    public static readonly Parser<HttpRequest> RequestParser =
        from reqLine in RequestLineParser
        from headers in HeaderParser.Many()
        from body in BodyParser.Optional()
        select new HttpRequest
        {
            Method = reqLine.Item1,
            Url = reqLine.Item2,
            Headers = new Dictionary<string, string>(headers),
            Body = body.GetOrDefault()
        };

    public static readonly Parser<IHttpSyntax> AnyParser =
        WhiteSpaceParser
        .Or(RequestParser)
        .Or(CommentParser)
        .Or(VariableParser)
        ;

    public static readonly Parser<IEnumerable<IHttpSyntax>> Parser =
        AnyParser.Many();
}
