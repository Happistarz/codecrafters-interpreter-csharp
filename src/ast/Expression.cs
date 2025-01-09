namespace AST;

public abstract class Expression
{
    public virtual T Accept<T>(IVisitor<T> _visitor)
    {
        throw new NotImplementedException();
    }
}

public interface IVisitor<out T>
{
    T VisitBinaryExpression(Binary _expression);
    T VisitGroupingExpression(Grouping _expression);
    T VisitLiteralExpression(Literal _expression);
    T VisitUnaryExpression(Unary _expression);
}

public class Binary(Expression _left, Token.Token _operator, Expression _right) : Expression
{
    public Expression  Left     { get; } = _left;
    public Token.Token Operator { get; } = _operator;
    public Expression  Right    { get; } = _right;

    public override T Accept<T>(IVisitor<T> _visitor)
    {
        return _visitor.VisitBinaryExpression(this);
    }
}

public class Grouping(Expression _expression) : Expression
{
    public Expression Expression { get; } = _expression;

    public override T Accept<T>(IVisitor<T> _visitor)
    {
        return _visitor.VisitGroupingExpression(this);
    }
}

public class Literal(object? _value) : Expression
{
    public object? Value { get; } = _value;

    public override T Accept<T>(IVisitor<T> _visitor)
    {
        return _visitor.VisitLiteralExpression(this);
    }
}

public class Unary(Token.Token _operator, Expression _right) : Expression
{
    public Token.Token Operator { get; } = _operator;
    public Expression  Right    { get; } = _right;

    public override T Accept<T>(IVisitor<T> _visitor)
    {
        return _visitor.VisitUnaryExpression(this);
    }
}