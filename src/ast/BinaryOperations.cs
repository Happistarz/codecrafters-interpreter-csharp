using Token;

namespace AST;

public interface IBinaryOperations
{
    public object? Apply(object? _left, object? _right);
}

public class Add : IBinaryOperations
{
    public object? Apply(object? _left, object? _right)
    {
        return (_left, _right) switch
        {
            (double left, double right) => left + right,
            (long left, long right) => left + right,
            (long left, double right) => left + right,
            (double left, long right) => left + right,
            (string left, string right) => left + right,
            _ => null
        };
    }
}

public class Subtract : IBinaryOperations
{
    public object? Apply(object? _left, object? _right)
    {
        return (_left, _right) switch
        {
            (double left, double right) => left - right,
            (long left, long right) => left - right,
            (long left, double right) => left - right,
            (double left, long right) => left - right,
            _ => null
        };
    }
}

public class Multiply : IBinaryOperations
{
    public object? Apply(object? _left, object? _right)
    {
        return (_left, _right) switch
        {
            (double left, double right) => left * right,
            (long left, long right) => left * right,
            (long left, double right) => left * right,
            (double left, long right) => left * right,
            _ => null
        };
    }
}

public class Divide : IBinaryOperations
{
    public object? Apply(object? _left, object? _right)
    {
        return (_left, _right) switch
        {
            (double left, double right) => left / right,
            (long left, long right) => left / right,
            (long left, double right) => (long)(left / right),
            (double left, long right) => left / right,
            _ => null
        };
    }
}

public static class BinaryOperationsFactory
{
    public static IBinaryOperations GetBinaryOperation(TokenType _operator)
    {
        return _operator switch
        {
            TokenType.PLUS => new Add(),
            TokenType.MINUS => new Subtract(),
            TokenType.STAR => new Multiply(),
            TokenType.SLASH => new Divide(),
            _ => throw new System.Exception("Unexpected binary operator.")
        };
    }
}