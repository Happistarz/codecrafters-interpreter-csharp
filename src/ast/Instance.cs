using AST.Classes;
using Parser;

namespace AST;

public class Instance(Class _class)
{
    private Class Class { get; } = _class;
    
    private readonly Dictionary<string, object?> _fields = new();
    
    public object? Get(Token.Token _name)
    {
        if (_fields.TryGetValue(_name.Lexeme, out var value))
        {
            return value;
        }

        var method = Class.FindMethod(_name.Lexeme);
        if (method != null) return method.Bind(this);

        throw new RuntimeError(_name, $"Undefined property '{_name.Lexeme}'.");
    }
    
    public void Set(Token.Token _name, object? _value)
    {
        _fields[_name.Lexeme] = _value;
    }

    public override string ToString()
    {
        return $"{Class.ClassStatement.Name} instance";
    }
}