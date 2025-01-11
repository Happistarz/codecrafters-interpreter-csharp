﻿namespace Parser;

using Token;
using AST.Expression;
using AST.Statement;

public class ParseError : Exception;

public class RuntimeError(Token _token, string _message) : Exception(_message)
{
    public readonly Token Token = _token;
}

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

    private Token Previous()
    {
        return _tokens[_current - 1];
    }

    private Token Advance()
    {
        if (!IsAtEnd()) _current++;
        return Previous();
    }

    private Token Consume(TokenType _type, string _message)
    {
        if (Check(_type)) return Advance();
        
        throw Error(_message, Peek());
    }

    private bool Match(params TokenType[] _types)
    {
        if (!_types.Any(Check)) return false;

        Advance();
        return true;
    }

    private bool Check(TokenType _type)
    {
        if (IsAtEnd()) return false;

        return Peek().Type == _type;
    }

    public List<Statement> Parse()
    {
        List<Statement> statements = [];
        while (!IsAtEnd())
        {
            try
            {
                statements.Add(Statement());
            }
            catch (ParseError)
            {
                Synchronize();
            }
        }
        
        return statements;
    }
    
    public Expression? ParseExpression()
    {
        try
        {
            return Expression();
        }
        catch (ParseError)
        {
            return null;
        }
    }
    
    private Statement Statement()
    {
        if (Match(TokenType.PRINT)) return PrintStatement();
        
        return ExpressionStatement();
    }
    
    private PrintStatement PrintStatement()
    {
        var value = Expression();
        Consume(TokenType.SEMICOLON, "Expect ';' after value.");
        return new PrintStatement(value);
    }
    
    private ExpressionStatement ExpressionStatement()
    {
        var value = Expression();
        Consume(TokenType.SEMICOLON, "Expect ';' after expression.");
        return new ExpressionStatement(value);
    }

    private Expression Expression()
    {
        return Equality();
    }

    private Expression Equality()
    {
        var expression = Comparison();

        while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
        {
            var @operator = Previous();
            var right     = Comparison();
            expression = new Binary(expression, @operator, right);
        }

        return expression;
    }

    private Expression Comparison()
    {
        var expression = Term();

        while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
        {
            var @operator = Previous();
            var right     = Term();
            expression = new Binary(expression, @operator, right);
        }

        return expression;
    }

    private Expression Term()
    {
        var expression = Factor();

        while (Match(TokenType.MINUS, TokenType.PLUS))
        {
            var @operator = Previous();
            var right     = Factor();
            expression = new Binary(expression, @operator, right);
        }

        return expression;
    }

    private Expression Factor()
    {
        var expression = Unary();

        while (Match(TokenType.SLASH, TokenType.STAR))
        {
            var @operator = Previous();
            var right     = Unary();
            expression = new Binary(expression, @operator, right);
        }

        return expression;
    }

    private Expression Unary()
    {
        if (!Match(TokenType.BANG, TokenType.MINUS)) return Primary();

        var @operator = Previous();
        var right     = Unary();
        return new Unary(@operator, right);
    }

    private Expression Primary()
    {
        if (Match(TokenType.FALSE)) return new Literal(false);
        if (Match(TokenType.TRUE)) return new Literal(true);
        if (Match(TokenType.NIL)) return new Literal(null);

        if (Match(TokenType.NUMBER, TokenType.STRING))
        {
            return new Literal(Previous().Literal);
        }

        if (!Match(TokenType.LEFT_PAREN)) throw Error("Expect expression.", Peek());

        var expression = Expression();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
        return new Grouping(expression);
    }
    
    private static ParseError Error(string _message, Token _token)
    {
        UTILS.Utils.Error(_token.Line, _token.Type == TokenType.EOF ? " at end" : $" at '{_token.Lexeme}'", _message);
        return new ParseError();
    }
    
    private void Synchronize()
    {
        Advance();
        
        while (!IsAtEnd())
        {
            if (Previous().Type == TokenType.SEMICOLON) return;
            
            switch (Peek().Type)
            {
                case TokenType.CLASS:
                case TokenType.FUN:
                case TokenType.VAR:
                case TokenType.FOR:
                case TokenType.IF:
                case TokenType.WHILE:
                case TokenType.PRINT:
                case TokenType.RETURN:
                    return;
            }
            
            Advance();
        }
    }
}