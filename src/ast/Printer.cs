using System.Text;
using AST.Classes;
using UTILS;
using AST.Expression;
using AST.Statement;
using AST.Operations;

namespace AST;

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
            builder.Append(statement?.Accept(new AstPrinter()) + "\n");
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
        return _expression.Value == null
            ? "nil"
            : Parenthesize($"{_expression.Value?.GetType()} {Utils.GetLiteralString(_expression.Value)}");
    }

    public string VisitUnaryExpression(Unary _expression)
    {
        return Parenthesize(_expression.Operator.Lexeme, _expression.Right);
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
        builder.Append(" call ");

        foreach (var argument in _expression.Arguments)
        {
            builder.Append(argument.Accept(this));
        }

        return Parenthesize(builder.ToString());
    }

    public string VisitThisExpression(This _expression)
    {
        return Parenthesize(_expression.Keyword.Lexeme);
    }

    public string VisitGetExpression(Get _expression)
    {
        var builder = new StringBuilder();

        builder.Append($"get {_expression.Name.Lexeme} {_expression.Object.Accept(this)}");
        
        return Parenthesize(builder.ToString());
    }

    public string VisitSetExpression(Set _expression)
    {
        var builder = new StringBuilder();

        builder.Append($"set {_expression.Name.Lexeme} {_expression.Object.Accept(this)} {_expression.Value.Accept(this)}");
        
        return Parenthesize(builder.ToString()); 
    }

    public string VisitNewExpression(New _expression)
    {
        var builder = new StringBuilder();
        
        builder.Append($"{_expression.Keyword.Lexeme} {_expression.Call.Accept(this)}");
        
        return Parenthesize(builder.ToString());
    }

    public string VisitWhileStatement(WhileStatement _expression)
    {
        var builder = new StringBuilder();

        builder.Append("while ");
        builder.Append(_expression.Condition.Accept(this));
        builder.Append(' ');
        builder.Append(_expression.Body.Accept(this));

        return Parenthesize(builder.ToString());
    }

    public string VisitFunctionStatement(FunctionStatement _expression)
    {
        var builder = new StringBuilder();

        builder.Append("fun ");
        builder.Append(_expression.Name.Lexeme);
        builder.Append(" (");

        foreach (var parameter in _expression.Parameters)
        {
            builder.Append(parameter.Type.Lexeme).Append(' ');
            builder.Append(parameter.Token.Lexeme).Append(',');
        }

        builder.Append(") ");
        foreach (var statement in _expression.Body)
        {
            builder.Append(statement?.Accept(this));
        }

        return Parenthesize(builder.ToString());
    }

    public string VisitReturnStatement(ReturnStatement _expression)
    {
        var builder = new StringBuilder();

        builder.Append("return");

        if (_expression.Value == null) return Parenthesize(builder.ToString());

        builder.Append(' ');
        builder.Append(_expression.Value.Accept(this));

        return Parenthesize(builder.ToString());
    }

    public string VisitClassStatement(ClassStatement _expression)
    {
        var builder = new StringBuilder();

        builder.Append($"class {_expression.Name.Lexeme}\n");

        foreach (var attribute in _expression.Attributes)
        {
            builder.Append(attribute?.Var.Accept(this));
        }

        builder.Append('\n');

        foreach (var method in _expression.Methods)
        {
            builder.Append(method?.Function.Accept(this));
        }

        return Parenthesize(builder.ToString());
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

        builder.Append(_expression.Type.Lexeme);
        builder.Append(' ');
        builder.Append(_expression.Name.Lexeme);

        if (_expression.Initializer == null) return Parenthesize(builder.ToString());

        builder.Append(" = ");
        builder.Append(_expression.Initializer.Accept(this));

        return Parenthesize(builder.ToString());
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

        builder.Append("if ");
        builder.Append(_expression.Condition.Accept(this));
        builder.Append(' ');
        builder.Append(_expression.ThenBranch.Accept(this));

        if (_expression.ElseBranch == null) return Parenthesize(builder.ToString());
        
        builder.Append(' ');
        builder.Append(_expression.ElseBranch.Accept(this));

        return Parenthesize(builder.ToString());
    }
    
    public string VisitImportStatement(ImportStatement _expression)
    {
        var builder = new StringBuilder();

        builder.Append("import ");
        builder.Append(_expression.Path.Lexeme);

        return Parenthesize(builder.ToString());
    }
}