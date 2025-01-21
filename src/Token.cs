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
    NUMBER,

    // Keywords
    AND,
    BOOL_TYPE,
    CLASS,
    DOUBLE_TYPE,
    ELSE,
    FALSE,
    FLOAT_TYPE,
    FOR,
    FUN,
    IF,
    INT_TYPE,
    NIL,
    OR,
    PRINT,
    RETURN,
    STRING_TYPE,
    SUPER,
    THIS,
    TRUE,
    VOID_TYPE,
    WHILE,

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
        return $"{Type} {Lexeme} {Utils.GetLiteralString(Literal,"null")}";
    }
}