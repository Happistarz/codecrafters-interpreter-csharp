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
    T VisitBlockStatement(BlockStatement           _expression);
    T VisitIfStatement(IfStatement                 _expression);
    T VisitWhileStatement(WhileStatement           _expression);
    T VisitFunctionStatement(FunctionStatement       _expression);
    T VisitReturnStatement(ReturnStatement         _expression);
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

public class BlockStatement(List<Statement?> _statements) : Statement
{
    public List<Statement?> Statements { get; } = _statements;

    public override T Accept<T>(IStatementVisitor<T> _statementVisitor)
    {
        return _statementVisitor.VisitBlockStatement(this);
    }
}

public class IfStatement(Expression _condition, Statement _thenBranch, Statement? _elseBranch) : Statement
{
    public Expression Condition  { get; } = _condition;
    public Statement  ThenBranch { get; } = _thenBranch;
    public Statement? ElseBranch { get; } = _elseBranch;

    public override T Accept<T>(IStatementVisitor<T> _statementVisitor)
    {
        return _statementVisitor.VisitIfStatement(this);
    }
}

public class WhileStatement(Expression _condition, Statement _body) : Statement
{
    public Expression Condition { get; } = _condition;
    public Statement  Body      { get; } = _body;

    public override T Accept<T>(IStatementVisitor<T> _statementVisitor)
    {
        return _statementVisitor.VisitWhileStatement(this);
    }
}

public class FunctionStatement(Token.Token _name, List<Token.Token> _parameters, List<Statement?> _body) : Statement
{
    public Token.Token       Name       { get; } = _name;
    public List<Token.Token> Parameters { get; } = _parameters;
    public List<Statement?>  Body       { get; } = _body;

    public override T Accept<T>(IStatementVisitor<T> _statementVisitor)
    {
        return _statementVisitor.VisitFunctionStatement(this);
    }
}

public class ReturnStatement(Token.Token _keyword, Expression? _value) : Statement
{
    public Token.Token Keyword { get; } = _keyword;
    public Expression? Value   { get; } = _value;

    public override T Accept<T>(IStatementVisitor<T> _statementVisitor)
    {
        return _statementVisitor.VisitReturnStatement(this);
    }
}