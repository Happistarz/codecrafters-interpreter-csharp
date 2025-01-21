namespace AST.Expression;

public abstract class Expression
{
    public virtual T Accept<T>(IExpressionVisitor<T> _expressionVisitor)
    {
        throw new NotImplementedException();
    }
}

public interface IExpressionVisitor<out T>
{
    T VisitBinaryExpression(Binary _expression);
    T VisitGroupingExpression(Grouping _expression);
    T VisitLiteralExpression(Literal _expression);
    T VisitUnaryExpression(Unary _expression);
    T VisitVariableExpression(Variable _expression);
    T VisitAssignExpression(Assign _expression);
    T VisitLogicalExpression(Logical _expression);
    T VisitCallExpression(Call _expression);
}

public class Binary(Expression _left, Token.Token _operator, Expression _right) : Expression
{
    public Expression  Left     { get; } = _left;
    public Token.Token Operator { get; } = _operator;
    public Expression  Right    { get; } = _right;

    public override T Accept<T>(IExpressionVisitor<T> _expressionVisitor)
    {
        return _expressionVisitor.VisitBinaryExpression(this);
    }
}

public class Grouping(Expression _expression) : Expression
{
    public Expression Expression { get; } = _expression;

    public override T Accept<T>(IExpressionVisitor<T> _expressionVisitor)
    {
        return _expressionVisitor.VisitGroupingExpression(this);
    }
}

public class Literal(object? _value) : Expression
{
    public object? Value { get; } = _value;

    public override T Accept<T>(IExpressionVisitor<T> _expressionVisitor)
    {
        return _expressionVisitor.VisitLiteralExpression(this);
    }
}

public class Unary(Token.Token _operator, Expression _right) : Expression
{
    public Token.Token Operator { get; } = _operator;
    public Expression  Right    { get; } = _right;

    public override T Accept<T>(IExpressionVisitor<T> _expressionVisitor)
    {
        return _expressionVisitor.VisitUnaryExpression(this);
    }
}

public class Variable(Token.Token _name) : Expression
{
    public Token.Token Name { get; } = _name;

    public override T Accept<T>(IExpressionVisitor<T> _expressionVisitor)
    {
        return _expressionVisitor.VisitVariableExpression(this);
    }
}

public class Assign(Token.Token _name, Expression _value) : Expression
{
    public Token.Token Name  { get; } = _name;
    public Expression  Value { get; } = _value;

    public override T Accept<T>(IExpressionVisitor<T> _expressionVisitor)
    {
        return _expressionVisitor.VisitAssignExpression(this);
    }
}

public class Logical(Expression _left, Token.Token _operator, Expression _right) : Expression
{
    public Expression  Left     { get; } = _left;
    public Token.Token Operator { get; } = _operator;
    public Expression  Right    { get; } = _right;

    public override T Accept<T>(IExpressionVisitor<T> _expressionVisitor)
    {
        return _expressionVisitor.VisitLogicalExpression(this);
    }
}

public class Call(Expression _callee, Token.Token _paren, List<Expression> _arguments) : Expression
{
    public Expression       Callee    { get; } = _callee;
    public Token.Token      Paren     { get; } = _paren;
    public List<Expression> Arguments { get; } = _arguments;
    
    public override T Accept<T>(IExpressionVisitor<T> _expressionVisitor)
    {
        return _expressionVisitor.VisitCallExpression(this);
    }
}