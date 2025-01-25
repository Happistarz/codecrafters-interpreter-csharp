using Parser;

namespace AST.Classes;

public class Class(ClassStatement _classStatement, Dictionary<string, Function> _functions) : ICallable
{
    public ClassStatement ClassStatement { get; } = _classStatement;

    private Dictionary<string, Function> Functions        { get; } = _functions;
    private Dictionary<string, object?>  StaticAttributes { get; } = new();

    public int Arity()
    {
        var ctor = FindMethod("constructor");
        return ctor?.Arity() ?? 0;
    }

    public object Call(InterpreterEvaluator _interpreter, List<object?> _arguments)
    {
        var instance = new Instance(this);
        var ctor     = FindMethod("constructor");

        ctor?.Bind(instance).Call(_interpreter, _arguments);

        return instance;
    }

    public Function? FindMethod(string _name)
    {
        return Functions.GetValueOrDefault(_name);
    }

    public object? FindStaticMember(string _name)
    {
        var method = ClassStatement.Methods.FirstOrDefault(_f => _f?.Static == true && _f.Function.Name.Lexeme == _name);
        return method is not null ? Functions[method.Function.Name.Lexeme] : StaticAttributes.GetValueOrDefault(_name);
    }

    public void SetStaticAttribute(Token.Token? _name, object? _value)
    {
        if (_name is null)
        {
            throw new RuntimeError(_name, "Static attribute name cannot be null.");
        }

        if (!StaticAttributes.TryAdd(_name.Lexeme, _value))
        {
            throw new RuntimeError(_name, $"Static attribute '{_name.Lexeme}' already exists.");
        }
    }
}