namespace Parser;

using Token;
using AST.Expression;
using AST.Statement;

// program        → statement* EOF ;
// statement      → exprStmt | printStmt | varStmt | block | ifStmt ;
// exprStmt       → expression ";" ;
// printStmt      → "print" expression ";" ;
// varStmt        → type IDENTIFIER ( "=" expression )? ";" ;
// block          → "{" declaration* "}" ;
// ifStmt         → "if" "(" expression ")" statement ( "else" statement )? ;
// whileStmt      → "while" "(" expression ")" statement ;
// forStmt        → "for" "(" ( varStmt | exprStmt | ";" ) expression? ";" expression? ")" statement ;
// funStmt        → "fun" type IDENTIFIER "(" parameters? ")" block ;
// parameters     → type IDENTIFIER ( "," type IDENTIFIER )* ;
// returnStmt     → "return" expression? ";" ;
// declaration    → statement ;

// type           → "float" | "double" | "int" | "string" | "bool" ;

// expression     → assignment ;
// assignment     → IDENTIFIER "=" assignment | equality ;
// logicalOr      → logicalAnd ( "or" logicalAnd )* ;
// logicalAnd     → equality ( "and" equality )* ;
// equality       → comparison ( ( "!=" | "==" ) comparison )* ;
// comparison     → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
// term           → factor ( ( "-" | "+" ) factor )* ;
// factor         → unary ( ( "/" | "*" ) unary )* ;
// unary          → ( "!" | "-" ) unary | primary ;
// call           → primary ( "(" arguments? ")" )* ;
// primary        → NUMBER | STRING | "true" | "false" | "nil" | "(" expression ")" | IDENTIFIER ;
// number         → literal ("D" | "F")? ;

public class ParseError : Exception;

public class RuntimeError(Token _token, string _message) : Exception(_message)
{
    public readonly Token Token = _token;
}

public class Parser(List<Token> _tokens)
{
    private static readonly List<TokenType> _VAR_TYPES =
        [TokenType.FLOAT_TYPE, TokenType.DOUBLE_TYPE, TokenType.INT_TYPE, TokenType.STRING_TYPE, TokenType.BOOL_TYPE];

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

    private Token ConsumeMany(List<TokenType> _types, string _message)
    {
        if (_types.Any(Check))
        {
            return Advance();
        }

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

    public List<Statement?> Parse()
    {
        List<Statement?> statements = [];
        while (!IsAtEnd())
        {
            var declaration = Declaration();
            if (declaration != null) statements.Add(declaration);
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

    private Statement? Declaration()
    {
        try
        {
            if (_VAR_TYPES.Contains(Peek().Type)) return VarStatement();

            if (Match(TokenType.FUN)) return FunctionStatement();
            return Statement();
        }
        catch (ParseError)
        {
            Synchronize();
            return null;
        }
    }

    private Statement Statement()
    {
        if (Match(TokenType.FOR)) return ForStatement();
        if (Match(TokenType.IF)) return IfStatement();
        if (Match(TokenType.PRINT)) return PrintStatement();
        if (Match(TokenType.RETURN)) return ReturnStatement();
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

    private ReturnStatement ReturnStatement()
    {
        var keyword = Previous();
        var value   = !(Check(TokenType.SEMICOLON)) ? Expression() : null;

        Consume(TokenType.SEMICOLON, "Expect ';' after value.");
        return new ReturnStatement(keyword, value);
    }

    private ExpressionStatement ExpressionStatement()
    {
        var value = Expression();
        Consume(TokenType.SEMICOLON, "Expect ';' after expression.");
        return new ExpressionStatement(value);
    }

    private VarStatement VarStatement()
    {
        var type = ConsumeMany(_VAR_TYPES, "Expect variable type.");
        var name = Consume(TokenType.IDENTIFIER, "Expect variable name.");

        Expression? initializer = null;

        if (Match(TokenType.EQUAL)) initializer = Expression();
        Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");

        return new VarStatement(type, name, initializer);
    }

    private FunctionStatement FunctionStatement()
    {
        var type = ConsumeMany([.._VAR_TYPES, TokenType.VOID_TYPE], "Expect return type.");
        var name = Consume(TokenType.IDENTIFIER, "Expect function name.");
        Consume(TokenType.LEFT_PAREN, "Expect '(' after function name.");
        
        List<(Token, Token)> parameters = [];
        
        if (!Check(TokenType.RIGHT_PAREN))
        {
            do
            {
                if (parameters.Count >= 255)
                {
                    Error("Cannot have more than 255 parameters.", Peek());
                }

                var paramType = ConsumeMany(_VAR_TYPES, "Expect parameter type.");
                var paramName = Consume(TokenType.IDENTIFIER, "Expect parameter name.");
                parameters.Add((paramType, paramName));
            } while (Match(TokenType.COMMA));
        }

        Consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");
        Consume(TokenType.LEFT_BRACE,  "Expect '{' before function body.");

        var body = BlockStatement();
        return new FunctionStatement(type, name, parameters, body.Statements);
    }

    private BlockStatement BlockStatement()
    {
        List<Statement?> statements = [];

        while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
        {
            statements.Add(Declaration());
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
        // else if (Match(TokenType.VAR))
        else if (_VAR_TYPES.Contains(Peek().Type))
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
        if (!Match(TokenType.BANG, TokenType.MINUS)) return Call();

        var @operator = Previous();
        var right     = Unary();
        return new Unary(@operator, right);
    }

    private Expression Call()
    {
        var expression = Primary();

        while (true)
        {
            if (Match(TokenType.LEFT_PAREN))
            {
                expression = FinishCall(expression);
            }
            else
            {
                break;
            }
        }

        return expression;
    }

    private Call FinishCall(Expression _callee)
    {
        List<Expression> arguments = [];

        if (!Check(TokenType.RIGHT_PAREN))
        {
            do
            {
                if (arguments.Count >= 255)
                {
                    Error("Cannot have more than 255 arguments.", Peek());
                }

                arguments.Add(Expression());
            } while (Match(TokenType.COMMA));
        }

        var paren = Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");

        return new Call(_callee, paren, arguments);
    }

    private Expression Primary()
    {
        if (Match(TokenType.FALSE)) return new Literal(false);
        if (Match(TokenType.TRUE)) return new Literal(true);
        if (Match(TokenType.NIL)) return new Literal(null);

        if (Match(TokenType.INT_TYPE, TokenType.FLOAT_TYPE, TokenType.DOUBLE_TYPE, TokenType.STRING, TokenType.STRING_TYPE)) 
            return new Literal(Previous().Literal);

        if (Match(TokenType.IDENTIFIER)) return new Variable(Previous());

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
                // case TokenType.VAR:
                case TokenType.FLOAT_TYPE:
                case TokenType.DOUBLE_TYPE:
                case TokenType.INT_TYPE:
                case TokenType.STRING_TYPE:
                case TokenType.BOOL_TYPE:
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