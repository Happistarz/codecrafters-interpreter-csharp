using AST.Statement;
using Env;

namespace AST;

public class Function(FunctionStatement _declaration, Definitions _closure, bool _isInitializer)
    : ICallable
{
    private FunctionStatement Declaration   { get; } = _declaration;
    private Definitions       Closure       { get; } = _closure;
    private bool              IsInitializer { get; } = _isInitializer;

    public int Arity()
    {
        return Declaration.Parameters.Count;
    }
    
    public object? Call(InterpreterEvaluator _interpreter, List<object?> _arguments)
    {
        var environment = new Definitions(Closure);
        
        for (var i = 0; i < Declaration.Parameters.Count; i++)
        {
            environment.Define(Declaration.Parameters[i].Token.Lexeme, _arguments[i]);
        }
        
        try
        {
            _interpreter.ExecuteBlock(Declaration.Body, environment);
        }
        catch (Return returnValue)
        {
            return IsInitializer ? Closure.GetAt(0, "this") : returnValue.Value;
        }
        
        return IsInitializer ? Closure.GetAt(0, "this") : null;
    }
    
    public Function Bind(Instance _instance)
    {
        var environment = new Definitions(Closure);
        environment.Define("this", _instance);
        
        return new Function(Declaration, environment, IsInitializer);
    }

    public override string ToString()
    {
        return $"<fn {Declaration.Name.Lexeme}>";
    }
}

public class Return(object? _value) : Exception
{
    public object? Value { get; } = _value;
}