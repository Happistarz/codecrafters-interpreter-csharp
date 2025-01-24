using Parser;

namespace Env;

public class Definitions(Definitions? _enclosing = null)
{
    private readonly Definitions? _enclosing = _enclosing;

    private readonly Dictionary<string, object?> _values = new();

    public void Assign(Token.Token _name, object? _value)
    {
        if (_values.TryGetValue(_name.Lexeme, out var value))
        {
            if (value != null && _value?.GetType() != value.GetType()) throw new RuntimeError(_name, $"Cannot assign '{_value}' to '{_name.Lexeme}' of type '{value.GetType()}'.");

            _values[_name.Lexeme] = _value;
            return;
        }

        if (_enclosing == null) throw new RuntimeError(_name, $"Undefined variable '{_name.Lexeme}'.");

        _enclosing?.Assign(_name, _value);
    }
    
    public void Define(string _name, object? _value)
    {
        _values[_name] = _value;
    }

    public object? Get(Token.Token _name)
    {
        if (_values.TryGetValue(_name.Lexeme, out var value))
        {
            return value;
        }

        if (_enclosing != null)
        {
            return _enclosing.Get(_name);
        }

        throw new RuntimeError(_name, $"Undefined variable '{_name.Lexeme}'.");
    }

    private Definitions Ancestor(int _distance)
    {
        var definitions = this;
        for (var i = 0; i < _distance; i++)
        {
            definitions = definitions._enclosing ?? definitions;
        }

        return definitions;
    }

    public void AssignAt(int _distance, Token.Token _name, object? _value)
    {
        Ancestor(_distance)._values[_name.Lexeme] = _value;
    }

    public object? GetAt(int _distance, string _name)
    {
        return Ancestor(_distance)._values[_name];
    }
}