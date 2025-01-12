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
    
    public static string Print(Statement.Statement _statement)
    {
        return _statement.Accept(new AstPrinter());
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
    
    public string VisitVariableExpression(Variable _expression)
    {
        return _expression.Name.Lexeme;
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
        return _expression.Expression.Accept(this);
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
}