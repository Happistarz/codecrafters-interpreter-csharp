﻿using Env;
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
    
    public static string Interpret(List<Statement.Statement?> _statements)
    {
        InterpreterEvaluator evaluator = new();
        try
        {
            foreach (var statement in _statements)
            {
                statement?.Accept(evaluator);
            }
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
    private Definitions _definitions = new();
    
    private Dictionary<string, ICallable> _nativesFunctions = new()
    {
        { "clock", new Clock() }
    };
    
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
                return Convert.ToDouble(right) * -1f;
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
        
        if (_expression.Left is Variable variable)
        {
            left = _definitions.Get(variable.Name);
        }
        
        if (_expression.Right is Variable variable1)
        {
            right = _definitions.Get(variable1.Name);
        }

        switch (_expression.Operator.Type)
        {
            case Token.TokenType.GREATER:
                CheckNumberOperands(_expression.Operator, left, right);
                return Convert.ToDouble(left) > Convert.ToDouble(right);
            case Token.TokenType.GREATER_EQUAL:
                CheckNumberOperands(_expression.Operator, left, right);
                return Convert.ToDouble(left) >= Convert.ToDouble(right);
            case Token.TokenType.LESS:
                CheckNumberOperands(_expression.Operator, left, right);
                return Convert.ToDouble(left) < Convert.ToDouble(right);
            case Token.TokenType.LESS_EQUAL:
                CheckNumberOperands(_expression.Operator, left, right);
                return Convert.ToDouble(left) <= Convert.ToDouble(right);
            case Token.TokenType.BANG_EQUAL:
                return !IsEqual(left, right);
            case Token.TokenType.EQUAL_EQUAL:
                return IsEqual(left, right);
        }
        
        var operation = BinaryOperationsFactory.GetBinaryOperation(_expression.Operator.Type).Apply(left, right);
        if (operation == null) throw new RuntimeError(_expression.Operator, "Operands must be two numbers or two strings.");
        return operation;
    }
    
    public object? VisitSetExpression(Set _expression)
    {
        var obj = _expression.Object.Accept(this);
        // if (!(obj is EnvInstance instance))
        // {
        //     throw new RuntimeError(_expression.Name, "Only instances have fields.");
        // }

        var value = _expression.Value.Accept(this);
        // instance.Set(_expression.Name.Lexeme, value);
        return value;
    }
    
    public object? VisitGetExpression(Get _expression)
    {
        var obj = _expression.Object.Accept(this);

        return obj;
        // if (obj is EnvInstance instance)
        // {
        //     return instance.Get(_expression.Name.Lexeme);
        // }
        //
        // throw new RuntimeError(_expression.Name, "Only instances have properties.");
    }
    
    public object? VisitVariableExpression(Variable _expression)
    {
        return _nativesFunctions.TryGetValue(_expression.Name.Lexeme, value: out var expression) ? expression : _definitions.Get(_expression.Name);
    }
    
    public object? VisitAssignExpression(Assign _expression)
    {
        var value = _expression.Value.Accept(this);
        _definitions.Assign(_expression.Name, value);
        return value;
    }
    
    public object? VisitLogicalExpression(Logical _expression)
    {
        var left = _expression.Left.Accept(this);

        if (_expression.Operator.Type == Token.TokenType.OR)
        {
            if (IsTruthy(left)) return left;
        }
        else
        {
            if (!IsTruthy(left)) return left;
        }

        return _expression.Right.Accept(this);
    }
    
    public object? VisitCallExpression(Call _expression)
    {
        var callee = _expression.Callee.Accept(this);
        var arguments = _expression.Arguments.Select(_argument => _argument.Accept(this)).ToList();

        if (callee is not ICallable function)
        {
            throw new RuntimeError(_expression.Paren, "Can only call functions and classes.");
        }

        if (arguments.Count != function.Arity())
        {
            throw new RuntimeError(_expression.Paren, $"Expected {function.Arity()} arguments but got {arguments.Count}.");
        }

        return function.Call(this, arguments);
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
        if (_operand is double or long) return;
        throw new RuntimeError(_operator, "Operand must be a number.");
    }

    private static void CheckNumberOperands(Token.Token _operator, object? _left, object? _right)
    {
        if (_left is double or long && _right is double or long) return;
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
    
    public object? VisitVarStatement(VarStatement _statement)
    {
        var value = _statement.Initializer?.Accept(this);
        _definitions.Define(_statement.Name.Lexeme, value);
        return null;
    }
    
    public object? VisitBlockStatement(BlockStatement _statement)
    {
        ExecuteBlock(_statement.Statements, new Definitions(_definitions));
        return null;
    }

    public void ExecuteBlock(List<Statement.Statement?> _statements, Definitions _def)
    {
        var previous = _definitions;
        try
        {
            _definitions = _def;
            foreach (var statement in _statements)
            {
                statement?.Accept(this);
            }
        }
        finally
        {
            _definitions = previous;
        }
    }
    
    public object? VisitIfStatement(IfStatement _statement)
    {
        if (IsTruthy(_statement.Condition.Accept(this)))
        {
            _statement.ThenBranch.Accept(this);
        }
        else
        {
            _statement.ElseBranch?.Accept(this);
        }

        return null;
    }

    public object? VisitWhileStatement(WhileStatement _statement)
    {
        while (IsTruthy(_statement.Condition.Accept(this)))
        {
            _statement.Body.Accept(this);
        }

        return null;
    }
    
    public object? VisitFunctionStatement(FunctionStatement _statement)
    {
        var function = new Function(_statement, _definitions,false);
        _definitions.Define(_statement.Name.Lexeme, function);
        return null;
    }

    public object? VisitReturnStatement(ReturnStatement _statement)
    {
        object? value = null;
        if (_statement.Value != null) value = _statement.Value.Accept(this);

        throw new Return(value);
    }
}