using Parser;

namespace Env;

public class Definitions
{
    public static readonly Definitions INSTANCE;
    
    static Definitions()
    {
        INSTANCE = new Definitions();
    }
    
    private readonly Dictionary<string, object?> _values = new();
    
    public void Assign(string _name, object? _value)
    {
        _values[_name] = _value;
    }
    
    public object? Get(Token.Token _name)
    {
        if (_values.TryGetValue(_name.Lexeme, out var value))
        {
            return value;
        }
        
        throw new RuntimeError(_name, $"Undefined variable '{_name.Lexeme}'.");
    }
}