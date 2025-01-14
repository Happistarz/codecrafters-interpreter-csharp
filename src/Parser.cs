namespace Parser;

using Token;
using AST.Expression;
using AST.Statement;

// program        → statement* EOF ;
// statement      → exprStmt | printStmt | varStmt | block | ifStmt ;
// exprStmt       → expression ";" ;
// printStmt      → "print" expression ";" ;
// varStmt        → "var" IDENTIFIER ( "=" expression )? ";" ;
// block          → "{" declaration* "}" ;
// ifStmt         → "if" "(" expression ")" statement ( "else" statement )? ;
// whileStmt      → "while" "(" expression ")" statement ;
// forStmt        → "for" "(" ( varStmt | exprStmt | ";" ) expression? ";" expression? ")" statement ;
// declaration    → statement ;

// expression     → assignment ;
// assignment     → IDENTIFIER "=" assignment | equality ;
// logicalOr      → logicalAnd ( "or" logicalAnd )* ;
// logicalAnd     → equality ( "and" equality )* ;
// equality       → comparison ( ( "!=" | "==" ) comparison )* ;
// comparison     → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
// term           → factor ( ( "-" | "+" ) factor )* ;
// factor         → unary ( ( "/" | "*" ) unary )* ;
// unary          → ( "!" | "-" ) unary | primary ;
// primary        → NUMBER | STRING | "true" | "false" | "nil" | "(" expression ")" | IDENTIFIER ;

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
                statements.Add(Declaration());
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

    private Statement Declaration()
    {
        if (Match(TokenType.VAR)) return VarStatement();
        return Statement();
    }

    private Statement Statement()
    {
        if (Match(TokenType.FOR)) return ForStatement();
        if (Match(TokenType.IF)) return IfStatement();
        if (Match(TokenType.PRINT)) return PrintStatement();
        if (Match(TokenType.WHILE)) return WhileStatement();
        if (Match(TokenType.LEFT_BRACE)) return BlockStatement();

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

    private VarStatement VarStatement()
    {
        var name = Consume(TokenType.IDENTIFIER, "Expect variable name.");

        Expression? initializer = null;

        if (Match(TokenType.EQUAL)) initializer = Expression();
        Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");

        return new VarStatement(name, initializer);
    }

    private BlockStatement BlockStatement()
    {
        List<Statement> statements = [];

        while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
        {
            statements.Add(Statement());
        }

        Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
        return new BlockStatement(statements);
    }

    private IfStatement IfStatement()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
        var condition = Expression();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");

        var        thenBranch = Statement();
        Statement? elseBranch = null;

        if (Match(TokenType.ELSE)) elseBranch = Statement();

        return new IfStatement(condition, thenBranch, elseBranch);
    }

    private WhileStatement WhileStatement()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
        var condition = Expression();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after while condition.");

        var body = Statement();

        return new WhileStatement(condition, body);
    }

    private Statement ForStatement()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");

        Statement? initializer;
        if (Match(TokenType.SEMICOLON))
        {
            initializer = null;
        }
        else if (Match(TokenType.VAR))
        {
            initializer = VarStatement();
        }
        else
        {
            initializer = ExpressionStatement();
        }

        Expression? condition = null;
        if (!Check(TokenType.SEMICOLON))
        {
            condition = Expression();
        }

        Consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");

        Expression? increment = null;
        if (!Check(TokenType.RIGHT_PAREN))
        {
            increment = Expression();
        }

        Consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses.");

        var body = Statement();

        if (increment != null)
        {
            body = new BlockStatement([body, new ExpressionStatement(increment)]);
        }

        condition ??= new Literal(true);
        body      =   new WhileStatement(condition, body);

        if (initializer != null)
        {
            body = new BlockStatement([initializer, body]);
        }

        return body;
    }

    private Expression Expression()
    {
        return Assignment();
    }

    private Expression Assignment()
    {
        var expression = LogicalOr();

        if (!Match(TokenType.EQUAL)) return expression;

        var equals = Previous();
        var value  = Assignment();

        if (expression is Variable variable)
        {
            return new Assign(variable.Name, value);
        }

        Error("Invalid assignment target.", equals);

        return expression;
    }

    private Expression LogicalOr()
    {
        var expression = LogicalAnd();

        while (Match(TokenType.OR))
        {
            var @operator = Previous();
            var right     = LogicalAnd();
            expression = new Logical(expression, @operator, right);
        }

        return expression;
    }

    private Expression LogicalAnd()
    {
        var expression = Equality();

        while (Match(TokenType.AND))
        {
            var @operator = Previous();
            var right     = Equality();
            expression = new Logical(expression, @operator, right);
        }

        return expression;
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

        if (Match(TokenType.IDENTIFIER))
        {
            return new Variable(Previous());
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