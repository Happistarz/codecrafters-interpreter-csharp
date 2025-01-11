using Parser;
using UTILS;

namespace AST;

using Expression;
using Statement;

public static class Interpreter
{
    public static string Interpret(Expression.Expression _expression)
    {
        try
        {
            var value = _expression.Accept(new InterpreterEvaluator());
            return Utils.GetLiteralString(value, _fixed: false);
        }
        catch (RuntimeError error)
        {
            Utils.RuntimeError(error.Token.Line, error.Message);
            return string.Empty;
        }
        catch (Exception exception)
        {
            return exception.Message;
        }
    }
    
    public static string Interpret(Statement.Statement _statement)
    {
        try
        {
            _statement.Accept(new InterpreterEvaluator());
            return string.Empty;
        }
        catch (RuntimeError error)
        {
            Utils.RuntimeError(error.Token.Line, error.Message);
            return string.Empty;
        }
        catch (Exception exception)
        {
            return exception.Message;
        }
    }
}

public class InterpreterEvaluator : IExpressionVisitor<object?>, IStatementVisitor<object?>
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

        switch (_expression.Operator.Type)
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

        switch (_expression.Operator.Type)
        {
            case Token.TokenType.MINUS:
                CheckNumberOperands(_expression.Operator, left, right);
                return (double)left - (double)right;
            case Token.TokenType.SLASH:
                CheckNumberOperands(_expression.Operator, left, right);
                return (double)left / (double)right;
            case Token.TokenType.STAR:
                CheckNumberOperands(_expression.Operator, left, right);
                return (double)left * (double)right;
            case Token.TokenType.PLUS:
                return left switch
                {
                    double dLeft when right is double dRight => dLeft + dRight,
                    string sLeft when right is string sRight => sLeft + sRight,
                    _ => throw new RuntimeError(_expression.Operator, "Operands must be two numbers or two strings.")
                };
            case Token.TokenType.GREATER:
                CheckNumberOperands(_expression.Operator, left, right);
                return (double)left > (double)right;
            case Token.TokenType.GREATER_EQUAL:
                CheckNumberOperands(_expression.Operator, left, right);
                return (double)left >= (double)right;
            case Token.TokenType.LESS:
                CheckNumberOperands(_expression.Operator, left, right);
                return (double)left < (double)right;
            case Token.TokenType.LESS_EQUAL:
                CheckNumberOperands(_expression.Operator, left, right);
                return (double)left <= (double)right;
            case Token.TokenType.BANG_EQUAL:
                return !IsEqual(left, right);
            case Token.TokenType.EQUAL_EQUAL:
                return IsEqual(left, right);
            default:
                return null;
        }
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

    private static void CheckNumberOperand(Token.Token _operator, object? _operand)
    {
        if (_operand is double) return;
        throw new RuntimeError(_operator, "Operand must be a number.");
    }

    private static void CheckNumberOperands(Token.Token _operator, object? _left, object? _right)
    {
        if (_left is double && _right is double) return;
        throw new RuntimeError(_operator, "Operands must be numbers.");
    }

    public object? VisitExpressionStatement(ExpressionStatement _statement)
    {
        return _statement.Expression.Accept(this);
    }

    public object? VisitPrintStatement(PrintStatement _statement)
    {
        var value = _statement.Expression.Accept(this);
        Console.WriteLine(Utils.GetLiteralString(value, _fixed: false));
        return null;
    }
}