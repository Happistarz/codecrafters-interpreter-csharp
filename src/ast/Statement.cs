namespace AST.Statement;

using Expression;

public class Statement
{
    public virtual T Accept<T>(IStatementVisitor<T> _statementVisitor)
    {
        throw new NotImplementedException();
    }
}

public interface IStatementVisitor<out T>
{
    T VisitExpressionStatement(ExpressionStatement _expression);
    T VisitPrintStatement(PrintStatement           _expression);
    T VisitVarStatement(VarStatement               _expression);
}

public class ExpressionStatement(Expression _expression) : Statement
{
    public Expression Expression { get; } = _expression;

    public override T Accept<T>(IStatementVisitor<T> _statementVisitor)
    {
        return _statementVisitor.VisitExpressionStatement(this);
    }
}

public class PrintStatement(Expression _expression) : Statement
{
    public Expression Expression { get; } = _expression;

    public override T Accept<T>(IStatementVisitor<T> _statementVisitor)
    {
        return _statementVisitor.VisitPrintStatement(this);
    }
}

public class VarStatement(Token.Token _name, Expression? _initializer) : Statement
{
    public Token.Token Name        { get; } = _name;
    public Expression? Initializer { get; } = _initializer;

    public override T Accept<T>(IStatementVisitor<T> _statementVisitor)
    {
        return _statementVisitor.VisitVarStatement(this);
    }
}