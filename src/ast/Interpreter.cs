using Parser;
using UTILS;

namespace AST;

public static class Interpreter
{
    public static string Interpret(Expression _expression)
    {
        try
        {
            var value = _expression.Accept(new ExpressionInterpreter());
            return Utils.GetLiteralString(value, _fixed: false);
        }
        catch (RuntimeError error)
        {
            Console.Error.WriteLine("{0}\n[line {1}]", error.Message, error.Token.Line);
            Environment.Exit(70);
            return string.Empty;
        }
        catch (Exception exception)
        {
            return exception.Message;
        }
    }
}

public class ExpressionInterpreter : IVisitor<object?>
{
    public object? VisitLiteralExpression(Literal _expression)
    {
        return _expression.Value;
    }

    public object? VisitGroupingExpression(Grouping _expression)
    {
        return _expression.Expression.Accept(this);
    }

    public object? VisitUnaryExpression(Unary _expression)
    {
        var right = _expression.Right.Accept(this);

        switch(_expression.Operator.Type)
        {
            case Token.TokenType.MINUS:
                CheckNumberOperand(_expression.Operator, right);
                return -(double)right;
            case Token.TokenType.BANG:
                return !IsTruthy(right);
            default:
                return null;
        }
    }

    public object? VisitBinaryExpression(Binary _expression)
    {
        var left  = _expression.Left.Accept(this);
        var right = _expression.Right.Accept(this);

        return _expression.Operator.Type switch
        {
            Token.TokenType.MINUS => (double)left - (double)right,
            Token.TokenType.SLASH => (double)left / (double)right,
            Token.TokenType.STAR  => (double)left * (double)right,
            Token.TokenType.PLUS => left switch
            {
                double number => number + (double)right,
                string text   => text   + right,
                _             => null
            },
            Token.TokenType.GREATER       => (double)left > (double)right,
            Token.TokenType.GREATER_EQUAL => (double)left >= (double)right,
            Token.TokenType.LESS          => (double)left < (double)right,
            Token.TokenType.LESS_EQUAL    => (double)left <= (double)right,
            Token.TokenType.BANG_EQUAL    => !IsEqual(left, right),
            Token.TokenType.EQUAL_EQUAL   => IsEqual(left, right),
            _                             => null
        };
    }

    private static bool IsTruthy(object? _value)
    {
        return _value switch
        {
            null         => false,
            bool boolean => boolean,
            _            => true
        };
    }

    private static bool IsEqual(object? _left, object? _right)
    {
        return _left switch
        {
            null when _right == null => true,
            null                     => false,
            _                        => _left.Equals(_right)
        };
    }
    
    private void CheckNumberOperand(Token.Token _operator, object? _operand)
    {
        if (_operand is double) return;
        throw new RuntimeError(_operator, "Operand must be a number.");
    }
}