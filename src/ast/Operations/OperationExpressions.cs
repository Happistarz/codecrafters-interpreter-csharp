using AST.Expression;

namespace AST.Operations;

public class Binary(Expression.Expression _left, Token.Token _operator, Expression.Expression _right)
    : Expression.Expression
{
    public Expression.Expression Left     { get; } = _left;
    public Token.Token           Operator { get; } = _operator;
    public Expression.Expression Right    { get; } = _right;

    public override T Accept<T>(IExpressionVisitor<T> _expressionVisitor)
    {
        return _expressionVisitor.VisitBinaryExpression(this);
    }
}

public class Grouping(Expression.Expression _expression) : Expression.Expression
{
    public Expression.Expression Expression { get; } = _expression;

    public override T Accept<T>(IExpressionVisitor<T> _expressionVisitor)
    {
        return _expressionVisitor.VisitGroupingExpression(this);
    }
}

public class Literal(object? _value) : Expression.Expression
{
    public object? Value { get; } = _value;

    public override T Accept<T>(IExpressionVisitor<T> _expressionVisitor)
    {
        return _expressionVisitor.VisitLiteralExpression(this);
    }
}

public class Unary(Token.Token _operator, Expression.Expression _right) : Expression.Expression
{
    public Token.Token           Operator { get; } = _operator;
    public Expression.Expression Right    { get; } = _right;

    public override T Accept<T>(IExpressionVisitor<T> _expressionVisitor)
    {
        return _expressionVisitor.VisitUnaryExpression(this);
    }
}

public class Logical(Expression.Expression _left, Token.Token _operator, Expression.Expression _right)
    : Expression.Expression
{
    public Expression.Expression Left     { get; } = _left;
    public Token.Token           Operator { get; } = _operator;
    public Expression.Expression Right    { get; } = _right;

    public override T Accept<T>(IExpressionVisitor<T> _expressionVisitor)
    {
        return _expressionVisitor.VisitLogicalExpression(this);
    }
}