using System.Globalization;
using System.Text;

namespace UTILS;

public static class Utils
{
    public static void Error(int _line, string _where, string _message)
    {
        Console.Error.WriteLine($"[line {_line}] Error{_where}: {_message}");
    }

    public static bool IsDigit(char _c)
    {
        return _c is >= '0' and <= '9';
    }

    public static bool IsAlpha(char _c)
    {
        return _c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_';
    }

    public static bool IsAlphaNumeric(char _c)
    {
        return IsAlpha(_c) || IsDigit(_c);
    }

    public static string GetLiteralString(object? _value, string _nullString = "nil", bool _fixed = true)
    {
        return _value switch
        {
            null                 => _nullString,
            string s             => s,
            double d when _fixed => d % 1 == 0 ? d.ToString("F1") : d.ToString(CultureInfo.InvariantCulture),
            double d             => d.ToString(CultureInfo.InvariantCulture),
            bool b               => b.ToString().ToLower(),
            _                    => throw new Exception("Unexpected value type.")
        };
    }
}