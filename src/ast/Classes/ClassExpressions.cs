using AST.Expression;
namespace AST.Classes;

public class This(Token.Token _keyword) : Expression.Expression
{
    public Token.Token Keyword { get; } = _keyword;
    
    public override T Accept<T>(IExpressionVisitor<T> _expressionVisitor)
    {
        return _expressionVisitor.VisitThisExpression(this);
    }
}

public class Get(Expression.Expression _object, Token.Token _name) : Expression.Expression
{
    public Expression.Expression Object { get; } = _object;
    public Token.Token           Name   { get; } = _name;
    
    public override T Accept<T>(IExpressionVisitor<T> _expressionVisitor)
    {
        return _expressionVisitor.VisitGetExpression(this);
    }
}

public class Set(Expression.Expression _object, Token.Token _name, Expression.Expression _value) : Expression.Expression
{
    public Expression.Expression Object { get; } = _object;
    public Token.Token           Name   { get; } = _name;
    public Expression.Expression Value  { get; } = _value;
    
    public override T Accept<T>(IExpressionVisitor<T> _expressionVisitor)
    {
        return _expressionVisitor.VisitSetExpression(this);
    }
}

public class New(Token.Token _keyword, Expression.Expression _call) : Expression.Expression
{
    public Token.Token           Keyword { get; } = _keyword;
    public Expression.Expression Call    { get; } = _call;
    
    public override T Accept<T>(IExpressionVisitor<T> _expressionVisitor)
    {
        return _expressionVisitor.VisitNewExpression(this);
    }
}