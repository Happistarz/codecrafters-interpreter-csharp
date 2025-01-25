using System.Globalization;
using UTILS;

namespace Token;

public class Tokenizer(string _content)
{
    private static readonly Dictionary<string, TokenType> _KEYWORDS =
        new()
        {
            { "and", TokenType.AND },
            { "else", TokenType.ELSE },
            { "false", TokenType.FALSE },
            { "for", TokenType.FOR },
            { "fun", TokenType.FUN },
            { "if", TokenType.IF },
            { "import", TokenType.IMPORT },
            { "nil", TokenType.NIL },
            { "or", TokenType.OR },
            { "print", TokenType.PRINT },
            { "return", TokenType.RETURN },
            { "true", TokenType.TRUE },

            { "string", TokenType.STRING_TYPE },
            { "int", TokenType.INT_TYPE },
            { "double", TokenType.DOUBLE_TYPE },
            { "float", TokenType.FLOAT_TYPE },
            { "bool", TokenType.BOOL_TYPE },
            { "void", TokenType.VOID_TYPE },
            { "while", TokenType.WHILE },

            { "class", TokenType.CLASS },
            { "constructor", TokenType.CONSTRUCTOR },
            { "new", TokenType.NEW },
            { "public", TokenType.PUBLIC },
            { "private", TokenType.PRIVATE },
            { "super", TokenType.SUPER },
            { "static", TokenType.STATIC },
            { "this", TokenType.THIS },
        };

    private readonly List<Token> _tokens = [];
    private          int         _start;
    private          int         _current;
    private          int         _line = 1;

    private bool IsAtEnd()
    {
        return _current >= _content.Length;
    }

    public List<Token> GetTokens()
    {
        return _tokens;
    }

    private char Peek()
    {
        return IsAtEnd() ? '\0' : _content[_current];
    }

    private char PeekNext()
    {
        return _current + 1 >= _content.Length ? '\0' : _content[_current + 1];
    }

    private char Advance()
    {
        return _content[_current++];
    }

    private void AddToken(TokenType _type, object? _literal = null)
    {
        var text = _content.Substring(_start, _current - _start);
        _tokens.Add(new Token(_type, text, _literal, _line));
    }

    private bool Match(char _expected)
    {
        if (IsAtEnd())
        {
            return false;
        }

        if (_content[_current] != _expected)
        {
            return false;
        }

        _current++;
        return true;
    }

    public void Scan()
    {
        while (!IsAtEnd())
        {
            _start = _current;
            ScanToken();
        }

        _tokens.Add(new Token(TokenType.EOF, "", default, _line));
    }

    private void ScanToken()
    {
        var c = Advance();
        switch (c)
        {
            case '(':
                AddToken(TokenType.LEFT_PAREN);
                break;
            case ')':
                AddToken(TokenType.RIGHT_PAREN);
                break;
            case '{':
                AddToken(TokenType.LEFT_BRACE);
                break;
            case '}':
                AddToken(TokenType.RIGHT_BRACE);
                break;
            case '*':
                AddToken(TokenType.STAR);
                break;
            case '+':
                AddToken(TokenType.PLUS);
                break;
            case '-':
                AddToken(TokenType.MINUS);
                break;
            case '.':
                AddToken(TokenType.DOT);
                break;
            case ',':
                AddToken(TokenType.COMMA);
                break;
            case ';':
                AddToken(TokenType.SEMICOLON);
                break;
            case '=':
                AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                break;
            case '!':
                AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                break;
            case '<':
                AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                break;
            case '>':
                AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                break;
            case '/':
                Comment(); // Comment handling                
                break;
            case '\'':
            case '"':
                String();
                break;
            case ' ':
            case '\r':
            case '\t':
                break;
            case '\n':
                _line++;
                break;
            default:
                if (Utils.IsDigit(c))
                {
                    Number();
                    break;
                }

                if (Utils.IsAlpha(c))
                {
                    Identifier();
                    break;
                }

                Error("Unexpected character: " + c);
                Program.HadError = true;
                break;
        }
    }

    private void String()
    {
        while (Peek() != '"' && !IsAtEnd())
        {
            if (Peek() == '\n')
            {
                _line++;
            }

            Advance();
        }

        if (IsAtEnd())
        {
            Error("Unterminated string.");
            Program.HadError = true;
            return;
        }

        Advance();

        var value = _content.Substring(_start + 1, _current - _start - 2);
        AddToken(TokenType.STRING, value);
    }

    private void Number()
    {
        while (Utils.IsDigit(Peek())) Advance();

        if (Peek() == '.' && Utils.IsDigit(PeekNext()))
        {
            Advance();
            while (Utils.IsDigit(Peek())) Advance();
        }

        object value;

        switch (Peek())
        {
            case 'd':
            case 'D':
                value = double.Parse(
                    _content.Substring(_start, _current - _start).Replace(',', '.')
                    , CultureInfo.InvariantCulture);
                Advance();
                AddToken(TokenType.DOUBLE_TYPE, value);
                break;

            case 'f':
            case 'F':
                value = float.Parse(
                    _content.Substring(_start, _current - _start).Replace(',', '.')
                    , CultureInfo.InvariantCulture);
                Advance();
                AddToken(TokenType.FLOAT_TYPE, value);
                break;
            default:
                if (_content.Substring(_start, _current - _start).Contains('.'))
                {
                    value = double.Parse(
                        _content.Substring(_start, _current - _start).Replace(',', '.')
                        , CultureInfo.InvariantCulture);
                    AddToken(TokenType.DOUBLE_TYPE, value);
                    break;
                }

                value = int.Parse(_content.AsSpan(_start, _current - _start), CultureInfo.InvariantCulture);
                AddToken(TokenType.INT_TYPE, value);
                break;
        }
    }

    private void Identifier()
    {
        while (Utils.IsAlphaNumeric(Peek())) Advance();

        var text = _content.Substring(_start, _current - _start);
        var type = _KEYWORDS.GetValueOrDefault(text, TokenType.IDENTIFIER);
        AddToken(type);
    }

    private void Comment()
    {
        if (Match('/')) // Single line comment
            while (Peek() != '\n' && !IsAtEnd())
                Advance();
        else if (Match('*')) // Multiline comment
        {
            while (Peek() != '*' && PeekNext() != '/' && !IsAtEnd())
            {
                if (Peek() == '\n')
                    _line++;
                Advance();
            }

            if (IsAtEnd())
            {
                Error("Unterminated comment.");
                Program.HadError = true;
                return;
            }

            Advance();
            Advance();
        }
        else
            AddToken(TokenType.SLASH);
    }

    private void Error(string _message)
    {
        Utils.Error(_line, "", _message, _content);
    }
}