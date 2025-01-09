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
    CLASS,
    ELSE,
    FALSE,
    FOR,
    FUN,
    IF,
    NIL,
    OR,
    PRINT,
    RETURN,
    SUPER,
    THIS,
    TRUE,
    VAR,
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
        return $"{Type} {Lexeme} {GetLiteralString()}";
    }
    
    private string GetLiteralString()
    {
        return Literal switch
        {
            null     => "null",
            string s => s,
            double d => d.ToString("F1"),
            _        => throw new Exception("Unknown literal type")
        };
    }
}