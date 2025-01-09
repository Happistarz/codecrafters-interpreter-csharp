using UTILS;

namespace Token
{
    public class Tokenizer(string _content)
    {
        public static readonly Dictionary<string, TokenType> KEYWORDS =
            new()
            {
                { "and", TokenType.AND },
                { "class", TokenType.CLASS },
                { "else", TokenType.ELSE },
                { "false", TokenType.FALSE },
                { "for", TokenType.FOR },
                { "fun", TokenType.FUN },
                { "if", TokenType.IF },
                { "nil", TokenType.NIL },
                { "or", TokenType.OR },
                { "print", TokenType.PRINT },
                { "return", TokenType.RETURN },
                { "super", TokenType.SUPER },
                { "this", TokenType.THIS },
                { "true", TokenType.TRUE },
                { "var", TokenType.VAR },
                { "while", TokenType.WHILE }
            };

        private string      content     = _content;
        private List<Token> tokens      = [];
        private int         start       = 0;
        private int         current     = 0;
        private int         line        = 1;
        private int         return_code = 0;

        private bool IsAtEnd()
        {
            return current >= content.Length;
        }

        public int GetReturnCode()
        {
            return return_code;
        }

        public List<Token> GetTokens()
        {
            return tokens;
        }

        private char Peek()
        {
            return IsAtEnd() ? '\0' : content[current];
        }

        private char PeekNext()
        {
            return current + 1 >= content.Length ? '\0' : content[current + 1];
        }

        private char Advance()
        {
            return content[current++];
        }

        private void AddToken(TokenType _type, object? _literal = null)
        {
            var text = content.Substring(start, current - start);
            tokens.Add(new Token(_type, text, _literal, line));
        }
        
        private bool Match(char _expected)
        {
            if (IsAtEnd())
            {
                return false;
            }

            if (content[current] != _expected)
            {
                return false;
            }

            current++;
            return true;
        }

        public void Scan()
        {
            while (!IsAtEnd())
            {
                start = current;
                ScanToken();
            }

            tokens.Add(new Token(TokenType.EOF, "", default, line));
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
                    if (Match('/'))
                        while (Peek() != '\n' && !IsAtEnd()) Advance();
                    else
                        AddToken(TokenType.SLASH);
                    break;
                case '\'':
                case '"':
                case ' ':
                case '\r':
                case '\t':
                    break;
                case '\n':
                    line++;
                    break;
                default:
                    
                    Utils.Error(line, "Unexpected character: " + c);
                    break;
            }
        }
    }
}