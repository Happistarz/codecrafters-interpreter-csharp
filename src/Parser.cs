namespace Parser;

using Token;

public class Parser(List<Token> _tokens)
{
    private int _current;

    private bool IsAtEnd()
    {
        return Peek().Type == TokenType.EOF;
    }

    private Token Peek()
    {
        return _tokens[_current];
    }

    private Token Advance()
    {
        if (!IsAtEnd())
        {
            _current++;
        }

        return _tokens[_current - 1];
    }

    public void Parse()
    {
        while (!IsAtEnd())
        {
            Console.WriteLine(Advance().Lexeme);
        }
    }
}