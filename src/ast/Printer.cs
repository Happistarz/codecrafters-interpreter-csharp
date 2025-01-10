using System.Text;
using UTILS;

namespace AST;

public static class Printer
{
    public static string Print(Expression _expression)
    {
        return _expression.Accept(new ExpressionPrinter());
    }
}

public class ExpressionPrinter : IVisitor<string>
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

    private string Parenthesize(string _name, params Expression[] _expressions)
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
}