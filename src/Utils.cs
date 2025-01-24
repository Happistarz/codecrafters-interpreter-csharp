using System.Globalization;
using AST;

namespace UTILS;

public static class Utils
{
    public static void Error(int _line, string _where, string _message, string? _content = null)
    {
        Console.Error.WriteLine($"[line {_line}] Error{_where}: {_message}");
        if (_content != null)
        {
            var contentLine = _content.Split('\n')[_line - 1];
            Console.Error.WriteLine(contentLine);
            // write the caret (^) under the error
            var caret = new string(' ', contentLine.IndexOf(contentLine.Trim(), StringComparison.Ordinal));
            Console.Error.WriteLine(caret + new string('^', contentLine.Trim().Length));
        }
        Program.HadError = true;
    }

    public static void RuntimeError(int _line, string _message)
    {
        Console.Error.WriteLine("{0}\n[line {1}]", _message, _line);
        Program.HadRuntimeError = true;
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
            double d             => $"{d.ToString(CultureInfo.InvariantCulture)}d",
            float f when _fixed  => f % 1 == 0 ? f.ToString("F1") : f.ToString(CultureInfo.InvariantCulture),
            float f              => $"{f.ToString(CultureInfo.InvariantCulture)}f",
            bool b               => b.ToString().ToLower(),
            _                    => _value.ToString() ?? string.Empty
        };
    }
}