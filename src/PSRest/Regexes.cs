using System.Text.RegularExpressions;

namespace PSRest;

partial class Regexes
{
    [GeneratedRegex("""{{(.*?)}}""")]
    public static partial Regex RestVariable();

    [GeneratedRegex("""\n\s*\r?\n""")]
    public static partial Regex EmptyLine();
}
