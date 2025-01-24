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
    FunctionStatement _function) : Statement.Statement
{
    public Token.Token       Visibility { get; } = _visibility;
    public FunctionStatement Function   { get; } = _function;

    public override T Accept<T>(IStatementVisitor<T> _statementVisitor)
    {
        return _statementVisitor.VisitMethodStatement(this);
    }
}

public class AttributeStatement(Token.Token _visibility, TypedToken _attribute) : Statement.Statement
{
    public Token.Token Visibility { get; } = _visibility;
    public TypedToken  Attribute  { get; } = _attribute;

    public override T Accept<T>(IStatementVisitor<T> _statementVisitor)
    {
        return _statementVisitor.VisitAttributeStatement(this);
    }
}