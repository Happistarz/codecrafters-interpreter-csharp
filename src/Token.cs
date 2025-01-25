using UTILS;

namespace Token;

public enum TokenType
{
    // Single-character tokens
    LEFT_PAREN,
    RIGHT_PAREN,
    LEFT_BRACE,
    RIGHT_BRACE,
    STAR,
    PLUS,
    MINUS,
    DOT,
    COMMA,
    SEMICOLON,
    SLASH,

    // One or two character tokens
    EQUAL,
    EQUAL_EQUAL,
    BANG,
    BANG_EQUAL,
    GREATER,
    GREATER_EQUAL,
    LESS,
    LESS_EQUAL,

    // Literals
    IDENTIFIER,
    STRING,

    // Keywords
    AND,
    ELSE,
    FALSE,
    FOR,
    FUN,
    IF,
    IMPORT,
    NIL,
    OR,
    PRINT,
    RETURN,
    TRUE,
    WHILE,

    // Types
    BOOL_TYPE,
    DOUBLE_TYPE,
    FLOAT_TYPE,
    INT_TYPE,
    STRING_TYPE,
    VOID_TYPE,

    // Class keywords
    CLASS,
    CONSTRUCTOR,
    NEW,
    PRIVATE,
    PUBLIC,
    SUPER,
    STATIC,
    THIS,

    EOF
}

public class Token(TokenType _type, string _lexeme, object? _literal, int _line)
{
    public TokenType Type    { get; } = _type;
    public string    Lexeme  { get; } = _lexeme;
    public object?   Literal { get; } = _literal;
    public int       Line    { get; } = _line;

    public override string ToString()
    {
        return $"{Type} {Lexeme} {Utils.GetLiteralString(Literal, "null")}";
    }
}

public struct TypedToken(Token _type, Token _token)
{
    public Token Type  { get; } = _type;
    public Token Token { get; } = _token;
}