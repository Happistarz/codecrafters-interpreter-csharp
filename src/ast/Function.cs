using AST.Statement;
using Env;

namespace AST;

public class Function : ICallable
{
    private FunctionStatement Declaration { get; }
    private Definitions Closure { get; }
    private bool IsInitializer { get; }
    
    public Function(FunctionStatement _declaration, Definitions _closure, bool _isInitializer)
    {
        Declaration = _declaration;
        Closure = _closure;
        IsInitializer = _isInitializer;
    }
    
    public int Arity()
    {
        return Declaration.Parameters.Count;
    }
    
    public object? Call(InterpreterEvaluator _interpreter, List<object?> _arguments)
    {
        var environment = new Definitions(Closure);
        
        for (var i = 0; i < Declaration.Parameters.Count; i++)
        {
            environment.Define(Declaration.Parameters[i].Lexeme, _arguments[i]);
        }
        
        // try
        // {
            _interpreter.ExecuteBlock(Declaration.Body, environment);
        // }
        // catch (Return returnValue)
        // {
        //     return IsInitializer ? Closure.GetAt(0, "this") : returnValue.Value;
        // }
        
        return IsInitializer ? Closure.GetAt(0, "this") : null;
    }

    public override string ToString()
    {
        return $"<fn {Declaration.Name.Lexeme}>";
    }
}