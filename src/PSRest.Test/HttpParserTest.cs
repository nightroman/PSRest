using Sprache;
using Xunit;

namespace PSRest.Test;

public class HttpParserTest
{
    [Theory]
    [InlineData("### bar", "## bar")]
    [InlineData("/// bar", "/ bar")]
    [InlineData("# bar\r\n", " bar")]
    public void Comment(string input, string expected)
    {
        var res = HttpParser.CommentParser.TryParse(input);
        Assert.True(res.WasSuccessful);
        Assert.Equal(expected, res.Value.Text);
    }

    const string Http1 = """
        # This is a comment
        // Another comment
        @token = abc123
        @user = john

        POST https://example.com/1
        Authorization: Bearer {{token}}
        Content-Type: application/json

        {
          "name": "{{user}}",
          "email": "john@example.com"
        }

        ### keep empty line after

        POST https://example.com/2
        Content-Type: application/json
        
        {
          "name": "{{user}}",
          "email": "john@example.com"
        }
        """;

    const string Http2 = """
        # Invoke-RestHttp should work
        //@param1 = 42 // file variables are not yet supported

        POST http://127.0.0.1:55000/show?param1=42
        content-type: application/json

        {
          "name": "sample",
          "time": "Wed, 21 Oct 2015 18:27:50 GMT"
        }
        """;

    [Theory]
    [InlineData(Http1, 9, 2)]
    [InlineData(Http2, 4, 1)]
    public void Http(string input, int nAll, int nRequest)
    {
        var res = HttpParser.Parser.TryParse(input);
        Assert.True(res.WasSuccessful);
        Assert.Equal(nAll, res.Value.Count());
        Assert.Equal(nRequest, res.Value.Count(x => x is HttpRequest));
    }

    const string Request0 = "GET https://example.com";

    const string Request1 = """
        POST https://example.com/1
        Authorization: Bearer {{token}}
        Content-Type: application/json

        {
          "name": "{{user}}",
          "email": "john@example.com"
        }
        """;

    const string Request2 = """
        POST https://example.com/2
        Authorization: Bearer {{token}}
        Content-Type: application/json

        {
          "name": "{{user}}",
          "email": "john@example.com"
        }

        ### next

        """;

    [Theory]
    [InlineData(Request0, 0, false)]
    [InlineData(Request1, 2, true)]
    [InlineData(Request2, 2, true)]
    public void Request(string input, int count, bool isBody)
    {
        var res = HttpParser.RequestParser.TryParse(input);
        Assert.True(res.WasSuccessful);
        Assert.Equal(count, res.Value.Headers.Count());
        if (isBody)
        {
            Assert.True(res.Value.Body.TrimEnd().EndsWith('}'));
        }
        else
        {
            Assert.Null(res.Value.Body);
        }
    }
}
