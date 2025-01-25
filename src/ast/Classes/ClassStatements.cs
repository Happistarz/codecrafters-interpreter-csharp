using AST.Statement;
using Token;

namespace AST.Classes;

public class ClassStatement(
    Token.Token               _name,
    List<AttributeStatement?> _attributes,
    List<MethodStatement?>    _methods) : Statement.Statement
{
    public Token.Token               Name        { get; } = _name;
    public List<AttributeStatement?> Attributes  { get; } = _attributes;
    public List<MethodStatement?>    Methods     { get; } = _methods;

    public override T Accept<T>(IStatementVisitor<T> _statementVisitor)
    {
        return _statementVisitor.VisitClassStatement(this);
    }
}
public class MethodStatement(
    Token.Token       _visibility,
    bool              _static,
    FunctionStatement _function) : Statement.Statement
{
    public Token.Token       Visibility { get; } = _visibility;
    public bool              Static     { get; } = _static;
    public FunctionStatement Function   { get; } = _function;

    public override T Accept<T>(IStatementVisitor<T> _statementVisitor)
    {
        throw new NotImplementedException();
    }
}

public class AttributeStatement(Token.Token _visibility, bool _static, VarStatement _var) : Statement.Statement
{
    public Token.Token Visibility { get; } = _visibility;
    public bool       Static     { get; } = _static;
    public VarStatement Var       { get; } = _var;
    public override T Accept<T>(IStatementVisitor<T> _statementVisitor)
    {
        throw new NotImplementedException();
    }
}