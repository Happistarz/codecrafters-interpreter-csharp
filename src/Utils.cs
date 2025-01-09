namespace UTILS;

public static class Utils
{
    public static void Error(int _line, string _message)
    {
        Console.Error.WriteLine($"[line {_line}] Error: {_message}");
    }
}