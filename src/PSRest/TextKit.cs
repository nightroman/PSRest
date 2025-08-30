using System;
using System.Xml.Linq;

namespace PSRest;

static class TextKit
{
    static TimeSpan ParseUnit(ReadOnlySpan<char> span)
    {
        return span switch
        {
            "d" => TimeSpan.FromDays(1),
            "h" => TimeSpan.FromHours(1),
            "m" => TimeSpan.FromMinutes(1),
            "s" => TimeSpan.FromSeconds(1),
            "ms" => TimeSpan.FromMilliseconds(1),
            "w" => TimeSpan.FromDays(7),
            "M" => TimeSpan.FromDays(30),
            "y" => TimeSpan.FromDays(365),
            _ => throw new Exception()
        };
    }

    internal static TimeSpan ParseOffset(ReadOnlySpan<char> name, ReadOnlySpan<char> span)
    {
        try
        {
            span = span.TrimStart();
            if (span.Length == 0)
                return TimeSpan.Zero;

            int index = span.IndexOf(' ');
            if (index < 0)
                throw new Exception();

            long offset = long.Parse(span[0..index]);
            span = span[(index + 1)..].TrimStart();

            TimeSpan unit = ParseUnit(span);
            return unit * offset;
        }
        catch (Exception)
        {
            throw new FormatException($"Parsing '{name}': expected '... int64 unit'.");
        }
    }

    internal static string ParseDatetime(ReadOnlySpan<char> name, string key, DateTime now)
    {
        var span = name;
        string format;
        try
        {
            // drop key
            span = span[key.Length..].TrimStart();
            if (span.Length == 0)
                throw new Exception();

            if (span.StartsWith(Const.DatetimeRfc1123))
            {
                format = "r";
                span = span[Const.DatetimeRfc1123.Length..].TrimStart();
            }
            else if (span.StartsWith(Const.DatetimeIso8601))
            {
                format = "u";
                span = span[Const.DatetimeIso8601.Length..].TrimStart();
            }
            else if (span[0] == '"')
            {
                span = span[1..];
                int index = span.IndexOf('"');
                if (index < 0)
                    throw new Exception();

                format = span[0..index].ToString();
                span = span[(index + 1)..].TrimStart();
            }
            else if (span[0] == '\'')
            {
                span = span[1..];
                int index = span.IndexOf('\'');
                if (index < 0)
                    throw new Exception();

                format = span[0..index].ToString();
                span = span[(index + 1)..].TrimStart();
            }
            else
            {
                throw new Exception();
            }
        }
        catch (Exception)
        {
            throw new FormatException($"""Parsing '{name}': expected '{key} rfc1123|iso8601|"custom format"|'custom format' ...'.""");
        }

        TimeSpan offset = span.Length == 0 ? TimeSpan.Zero : ParseOffset(name, span);
        return (now + offset).ToString(format);
    }
}
