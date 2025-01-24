
namespace AST.Classes;

public class Class(ClassStatement _classStatement, Dictionary<string, Function> _functions) : ICallable
{
    public  ClassStatement ClassStatement   { get; } = _classStatement;
    private Dictionary<string, Function> Functions { get; } = _functions;

    public int Arity()
    {
        var ctor = FindMethod("constructor");
        return ctor?.Arity() ?? 0;
    }

    public object Call(InterpreterEvaluator _interpreter, List<object?> _arguments)
    {
        var instance = new Instance(this);
        var ctor = FindMethod("constructor");

        ctor?.Bind(instance).Call(_interpreter, _arguments);

        return instance;
    }
    
    public Function? FindMethod(string _name)
    {
        return Functions.GetValueOrDefault(_name);
    }
}