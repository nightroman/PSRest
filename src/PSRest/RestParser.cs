using Sprache;

namespace PSRest;

public static class RestParser
{
    static T Catch<T>(Func<T> @try, Func<Exception, T> @catch)
    {
        try
        {
            return @try();
        }
        catch (Exception ex)
        {
            return @catch(ex);
        }
    }

    public static readonly Parser<AnySyntax> WhiteSpaceParser =
        from ws1 in Parse.WhiteSpace.AtLeastOnce()
        select (AnySyntax)null!;

    public static readonly Parser<string> PromptParser =
        from prompt in Parse.String("@prompt").Token()
        from name in Parse.LetterOrDigit.AtLeastOnce().Text()
        from ws1 in Parse.Chars(" \t").Many()
        select name;

    public static readonly Parser<RestComment> CommentParser =
        from ws1 in Parse.WhiteSpace.Many()
        from start in Parse.String("#").Or(Parse.String("//")).Text()
        from prompt in PromptParser.Optional()
        from text in Parse.CharExcept("\r\n").Many().Text()
        from _ in Parse.LineTerminator
        select new RestComment
        {
            Start = start,
            Text = text,
            Prompt = prompt.GetOrDefault()
        };

    public static readonly Parser<RestVariable> VariableParser =
        from at in Parse.Char('@')
        from name in Parse.LetterOrDigit.AtLeastOnce().Text().Token()
        from eq in Parse.Char('=').Token()
        from value in Parse.CharExcept("\r\n").Many().Text()
        from _ in Parse.LineTerminator
        select new RestVariable
        {
            Name = name,
            Value = value
        };

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
        Parse.CharExcept(" \r\n").Many().Text().Token();

    public static readonly Parser<Version> HttpVersionParser =
        from http in Parse.String("HTTP/")
        from version in Parse.CharExcept("\r\n").AtLeastOnce().Text().Token()
        select Catch(() => new Version(version), ex => throw new FormatException($"HTTP version '{version}': {ex.Message}", ex));

    public static readonly Parser<RestOperation> OperationParser =
        from method in MethodParser
        from ws1 in Parse.WhiteSpace.AtLeastOnce()
        from url in UrlParser
        from version in HttpVersionParser.Optional()
        select new RestOperation
        {
            Method = method,
            Url = url,
            Version = version.GetOrDefault()
        };

    public static readonly Parser<RestHeader> HeaderParser =
        from key in Parse.Char(c => !char.IsWhiteSpace(c) && c != ':', "key").Many().Text().Token()
        from colon in Parse.Char(':')
        from value in Parse.CharExcept("\r\n").Many().Text()
        from _ in Parse.LineTerminator
        select new RestHeader
        {
            Key = key,
            Value = value.Trim()
        };

    public static readonly Parser<string> BodyLineParser =
        from next in Parse.Not(Parse.String("###"))
        from line in Parse.CharExcept("\r\n").Many().Text()
        from _ in Parse.LineTerminator
        select line;

    public static readonly Parser<string> BodyParser =
        from blank in Parse.LineEnd.AtLeastOnce()
        from lines in BodyLineParser.Many()
        select string.Join('\n', lines);

    public static readonly Parser<RestRequest> RequestParser =
        from operation in OperationParser
        from headers in HeaderParser.Many()
        from body in BodyParser.Optional()
        select new RestRequest
        {
            Operation = operation,
            Headers = [.. headers],
            Body = body.GetOrDefault()
        };

    public static readonly Parser<AnySyntax> AnyParser =
        WhiteSpaceParser
        .Or(RequestParser)
        .Or(CommentParser.Positioned())
        .Or(VariableParser)
        ;

    public static readonly Parser<IEnumerable<AnySyntax>> Parser =
        AnyParser.Many();
}
