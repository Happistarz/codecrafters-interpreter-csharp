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
    T VisitPrintStatement(PrintStatement _expression);
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