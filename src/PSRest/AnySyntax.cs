using Sprache;

namespace PSRest;

public abstract record AnySyntax
{
    public Position? Pos { get; private set; }
    public int Len { get; private set; }

    public void SetPos(Position pos, int len)
    {
        Pos = pos;
        Len = len;
    }
}

static class Parse2
{
    /// <summary>
    /// A parser that sets the position on succsessful match.
    /// </summary>
    public static Parser<T> Positioned<T>(this Parser<T> parser) where T : AnySyntax
    {
        return i =>
        {
            var r = parser(i);
            if (r.WasSuccessful)
                r.Value.SetPos(Position.FromInput(i), r.Remainder.Position - i.Position);
            return r;
        };
    }
}
