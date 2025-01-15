namespace AST;

public interface ICallable
{
    public int Arity();
    public object? Call(InterpreterEvaluator _interpreter, List<object?> _arguments);
}

public class Clock : ICallable
{
    public int Arity()
    {
        return 0;
    }

    public object? Call(InterpreterEvaluator _interpreter, List<object?> _arguments)
    {
        return (DateTime.UtcNow - new DateTime(1970,1,1)).TotalSeconds;
    }

    public override string ToString()
    {
        return "<native fn>";
    }
}