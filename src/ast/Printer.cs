using System.Text;
using UTILS;

namespace AST;
using Expression;
using Statement;

public static class Printer
{
    public static string Print(Expression.Expression _expression)
    {
        return _expression.Accept(new AstPrinter());
    }
    
    public static string Print(List<Statement.Statement?> _statements)
    {
        var builder = new StringBuilder();

        foreach (var statement in _statements)
        {
            builder.Append(statement?.Accept(new AstPrinter()));
        }

        return builder.ToString();
    }
}

public class AstPrinter : IExpressionVisitor<string>, IStatementVisitor<string>
{
    public string VisitBinaryExpression(Binary _expression)
    {
        return Parenthesize(_expression.Operator.Lexeme, _expression.Left, _expression.Right);
    }

    public string VisitGroupingExpression(Grouping _expression)
    {
        return Parenthesize("group", _expression.Expression);
    }

    public string VisitLiteralExpression(Literal _expression)
    {
        return _expression.Value == null ? "nil" : Utils.GetLiteralString(_expression.Value);
    }

    public string VisitUnaryExpression(Unary _expression)
    {
        return Parenthesize(_expression.Operator.Lexeme, _expression.Right);
    }
    
    public string VisitSetExpression(Set _expression)
    {
        return Parenthesize("set", _expression.Object, _expression.Value);
    }
    
    public string VisitGetExpression(Get _expression)
    {
        return Parenthesize("get", _expression.Object);
    }
    
    public string VisitVariableExpression(Variable _expression)
    {
        return _expression.Name.Lexeme;
    }
    
    public string VisitAssignExpression(Assign _expression)
    {
        var builder = new StringBuilder();
        
        builder.Append(_expression.Name.Lexeme).Append(" = ");
        builder.Append(_expression.Value.Accept(this));
        
        return Parenthesize(builder.ToString());
    }
    
    public string VisitLogicalExpression(Logical _expression)
    {
        return Parenthesize(_expression.Operator.Lexeme, _expression.Left, _expression.Right);
    }
    
    public string VisitCallExpression(Call _expression)
    {
        var builder = new StringBuilder();
        
        builder.Append(_expression.Callee.Accept(this));
        builder.Append(" (call ");
        
        foreach (var argument in _expression.Arguments)
        {
            builder.Append(argument.Accept(this));
        }
        
        builder.Append(')');
        
        return builder.ToString();
    }
    
    public string VisitWhileStatement(WhileStatement _expression)
    {
        var builder = new StringBuilder();

        builder.Append("(while ");
        builder.Append(_expression.Condition.Accept(this));
        builder.Append(' ');
        builder.Append(_expression.Body.Accept(this));
        builder.Append(')');

        return builder.ToString();
    }

    public string VisitFunctionStatement(FunctionStatement _expression)
    {
        var builder = new StringBuilder();

        builder.Append("(fun ");
        builder.Append(_expression.Name.Lexeme);
        builder.Append(" (");

        foreach (var parameter in _expression.Parameters)
        {
            builder.Append(parameter.Lexeme).Append(' ');
        }

        builder.Append(") ");
        foreach (var statement in _expression.Body)
        {
            builder.Append(statement?.Accept(this));
        }
        builder.Append(')');

        return builder.ToString();
    }

    private string Parenthesize(string _name, params Expression.Expression[] _expressions)
    {
        var builder = new StringBuilder();

        builder.Append('(').Append(_name);

        foreach (var expression in _expressions)
        {
            builder.Append(' ');
            builder.Append(expression.Accept(this));
        }

        builder.Append(')');

        return builder.ToString();
    }
    
    public string VisitExpressionStatement(ExpressionStatement _expression)
    {
        return _expression.Expression.Accept(this);
    }
    
    public string VisitPrintStatement(PrintStatement _expression)
    {
        var builder = new StringBuilder();
        
        builder.Append("print ");
        builder.Append(_expression.Expression.Accept(this));
        
        return Parenthesize(builder.ToString());
    }
    
    public string VisitVarStatement(VarStatement _expression)
    {
        var builder = new StringBuilder();

        builder.Append("(var ").Append(_expression.Name.Lexeme);

        if (_expression.Initializer != null)
        {
            builder.Append(" = ");
            builder.Append(_expression.Initializer.Accept(this));
        }

        builder.Append(')');

        return builder.ToString();
    }
    
    public string VisitBlockStatement(BlockStatement _expression)
    {
        var builder = new StringBuilder();

        builder.Append("{block ");

        foreach (var statement in _expression.Statements)
        {
            builder.Append(statement?.Accept(this));
        }

        builder.Append('}');

        return builder.ToString();
    }
    
    public string VisitIfStatement(IfStatement _expression)
    {
        var builder = new StringBuilder();

        builder.Append("(if ");
        builder.Append(_expression.Condition.Accept(this));
        builder.Append(' ');
        builder.Append(_expression.ThenBranch.Accept(this));

        if (_expression.ElseBranch != null)
        {
            builder.Append(' ');
            builder.Append(_expression.ElseBranch.Accept(this));
        }

        builder.Append(')');

        return builder.ToString();
    }
}