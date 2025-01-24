using AST.Classes;
using Env;
using Parser;
using UTILS;
using AST.Statement;
using AST.Expression;
using AST.Operations;

namespace AST;

public static class Interpreter
{
    public static string Interpret(Expression.Expression? _expression)
    {
        try
        {
            var value = _expression?.Accept(new InterpreterEvaluator());
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

    private readonly Dictionary<string, ICallable> _nativesFunctions = new()
    {
        { "clock", new Clock() },
        { "currentDateTime", new CurrentDateTime() }
    };

    public object? VisitLiteralExpression(Literal _expression)
    {
        return _expression.Value;
    }

    public object? VisitGroupingExpression(Grouping _expression)
    {
        return _expression.Expression.Accept(this);
    }

    public object VisitUnaryExpression(Unary _expression)
    {
        var right = _expression.Right.Accept(this);

        var operation = OperationsFactory.GetOperation(_expression.Operator.Type).Apply(right, null);
        if (operation == null) throw new RuntimeError(_expression.Operator, "Operand must be a number.");
        return operation;
    }

    public object VisitBinaryExpression(Binary _expression)
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

        var operation = OperationsFactory.GetOperation(_expression.Operator.Type).Apply(left, right);
        if (operation == null)
            throw new RuntimeError(_expression.Operator, "Operands must be two numbers or two strings.");
        return operation;
    }

    public object VisitVariableExpression(Variable _expression)
    {
        return LookUpVariable(_expression.Name, _expression);
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
        var callee    = _expression.Callee.Accept(this);
        var arguments = _expression.Arguments.Select(_argument => _argument.Accept(this)).ToList();

        if (callee is not ICallable function)
        {
            throw new RuntimeError(_expression.Paren, "Can only call functions and classes.");
        }

        if (arguments.Count != function.Arity())
        {
            throw new RuntimeError(_expression.Paren,
                                   $"Expected {function.Arity()} arguments but got {arguments.Count}.");
        }

        return function.Call(this, arguments);
    }

    public object VisitThisExpression(This _expression)
    {
        return LookUpVariable(_expression.Keyword, _expression);
    }

    public object? VisitGetExpression(Get _expression)
    {
        var value = _expression.Object.Accept(this);
        if (value is Instance instance)
        {
            return instance.Get(_expression.Name);
        }
        
        throw new RuntimeError(_expression.Name, "Only instances have properties.");
    }

    public object? VisitSetExpression(Set _expression)
    {
        var objectInstance = _expression.Object.Accept(this);
        if (objectInstance is not Instance instance)
        {
            throw new RuntimeError(_expression.Name, "Only instances have fields.");
        }

        var value = _expression.Value.Accept(this);
        instance.Set(_expression.Name, value);
        return value;
    }

    public object VisitNewExpression(New _expression)
    {
        var classInstance = _expression.Call.Accept(this);
        if (classInstance is not Instance instance)
        {
            throw new RuntimeError(_expression.Keyword, "Can only instantiate classes.");
        }

        return instance;
    }
    
    private object LookUpVariable(Token.Token _name, Expression.Expression _expression)
    {
        if (_nativesFunctions.TryGetValue(_name.Lexeme, out var expression))
        {
            return expression;
        }

        var value = _definitions.Get(_name);
        if (value != null) return value;

        throw new RuntimeError(_name, $"Undefined variable '{_name.Lexeme}'.");
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
        var function = new Function(_statement, _definitions, false);
        _definitions.Define(_statement.Name.Lexeme, function);

        HasValidReturn(_statement);

        return null;
    }

    private static void HasValidReturn(FunctionStatement _statement)
    {
        var returnStatement = GetBodyReturnStatement(_statement.Body);
        if (_statement.Type.Lexeme == "void")
        {
            foreach (var statement in returnStatement)
            {
                if (statement.Value is null) continue;

                throw new RuntimeError(_statement.Type, "Cannot return a value from a void function.");
            }
        }
        else if (returnStatement.Count == 0)
        {
            throw new RuntimeError(_statement.Type, "Missing return statement.");
        }
    }

    private static List<ReturnStatement> GetBodyReturnStatement(List<Statement.Statement?> _statements)
    {
        List<ReturnStatement> returnStatements = [];
        foreach (var statement in _statements)
        {
            switch (statement)
            {
                case ReturnStatement returnStatement:
                    returnStatements.Add(returnStatement);
                    break;

                case BlockStatement block:
                    returnStatements.AddRange(GetBodyReturnStatement(block.Statements));
                    break;
                case IfStatement ifStatement:
                    returnStatements.AddRange(GetBodyReturnStatement([ifStatement.ThenBranch]));
                    if (ifStatement.ElseBranch != null)
                        returnStatements.AddRange(GetBodyReturnStatement([ifStatement.ElseBranch]));
                    break;
                case WhileStatement whileStatement:
                    returnStatements.AddRange(GetBodyReturnStatement([whileStatement.Body]));
                    break;
            }
        }

        return returnStatements;
    }

    public object VisitReturnStatement(ReturnStatement _statement)
    {
        object? value                       = null;
        if (_statement.Value != null) value = _statement.Value.Accept(this);

        throw new Return(value);
    }

    public object? VisitClassStatement(ClassStatement _expression)
    {
        _definitions.Define(_expression.Name.Lexeme, null);
        
        var methods = new Dictionary<string, Function>();
        
        foreach (var method in _expression.Methods)
        {
            var function = new Function(method?.Function ?? throw new InvalidOperationException(), _definitions, method.Function.Name.Lexeme == "constructor");
            methods[method.Function.Name.Lexeme ?? throw new InvalidOperationException()] = function;
        }
        
        var classInstance = new Class(_expression, methods);
        _definitions.Assign(_expression.Name, classInstance);
        
        return null;
    }
    
    public object? VisitMethodStatement(MethodStatement _expression)
    {
        return null;
    }

    public object? VisitAttributeStatement(AttributeStatement _expression)
    {
        return null;
    }
}