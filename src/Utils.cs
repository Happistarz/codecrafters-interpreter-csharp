namespace UTILS;

public static class Utils
{
    public static void Error(int _line, string _message)
    {
        Console.Error.WriteLine($"[line {_line}] Error: {_message}");
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
}