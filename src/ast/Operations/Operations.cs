using Token;

namespace AST.Operations;

public interface IOperations
{
    public object? Apply(object? _left, object? _right);
}

public class BinaryOperation(Func<dynamic?, dynamic?, dynamic> _operation) : IOperations
{
    public object? Apply(object? _left, object? _right)
    {
        try
        {
            return _operation(_left, _right);
        }
        catch
        {
            return null;
        }
    }
}

public class ComparisonOperation(Func<dynamic?, dynamic?, bool> _comparison) : IOperations
{
    public object? Apply(object? _left, object? _right)
    {
        try
        {
            return _comparison(_left, _right);
        }
        catch
        {
            return null;
        }
    }
}

public class LogicalOperation(Func<bool, bool, bool> _logical) : IOperations
{
    public object? Apply(object? _left, object? _right)
    {
        return (_left, _right) switch
        {
            (bool left, bool right) => _logical(left, right),
            _ => null
        };
    }
}


public static class OperationsFactory
{
    private static readonly Dictionary<TokenType, IOperations> _OPERATIONS_MAP = new()
    {
        { TokenType.PLUS, new BinaryOperation((_l, _r) => _l + _r) },
        { TokenType.MINUS, new BinaryOperation((_l, _r) => _l - _r) },
        { TokenType.STAR, new BinaryOperation((_l, _r) => _l * _r) },
        { TokenType.SLASH, new BinaryOperation((_l, _r) => _l / _r) },
        { TokenType.GREATER, new ComparisonOperation((_l, _r) => _l > _r) },
        { TokenType.GREATER_EQUAL, new ComparisonOperation((_l, _r) => _l >= _r) },
        { TokenType.LESS, new ComparisonOperation((_l, _r) => _l < _r) },
        { TokenType.LESS_EQUAL, new ComparisonOperation((_l, _r) => _l <= _r) },
        { TokenType.EQUAL_EQUAL, new ComparisonOperation((_l, _r) => _l == _r) },
        { TokenType.BANG_EQUAL, new ComparisonOperation((_l, _r) => _l != _r) },
        { TokenType.AND, new LogicalOperation((_l, _r) => _l && _r) },
        { TokenType.OR, new LogicalOperation((_l, _r) => _l || _r) }
    };
    public static IOperations GetOperation(TokenType _operator)
    {
        if (_OPERATIONS_MAP.TryGetValue(_operator, out var operation))
        {
            return operation;
        }
        
        throw new Exception($"Unsupported operation: {_operator}");
    }
}