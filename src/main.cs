using AST;
using Token;

internal class Program
{
    public static           bool         HadError        = false;
    public static           bool         HadRuntimeError = false;
    private static readonly List<string> _COMMANDS       = ["tokenize", "parse", "evaluate", "run"];

    public static void Main(string[] _args)
    {
        if (_args.Length < 2)
        {
            Console.Error.WriteLine("Usage: ./your_program.sh tokenize <filename>");
            Environment.Exit(1);
        }

        var command  = _args[0];
        var filename = _args[1];


        var fileContents = File.ReadAllText(filename);


        //Uncomment this block to pass the first stage
        if (!string.IsNullOrEmpty(fileContents))
        {
            Run(command, fileContents);

            if (HadError) Environment.Exit(65);

            if (HadRuntimeError) Environment.Exit(70);
        }
        else
        {
            Console.WriteLine("EOF  null");
        }

        Environment.Exit(0);
    }

    private static void Run(string _command, string _source)
    {
        // if (!_COMMANDS.Contains(_command))
        // {
        //     Console.Error.WriteLine($"Unknown command: {_command}");
        //     Environment.Exit(1);
        // }
        //
        // Tokenizer tokenizer = new(_source);
        // tokenizer.Scan();
        //
        // var tokens = tokenizer.GetTokens();
        //
        // if (HadError) return;
        //
        // if (_command == _COMMANDS[0])
        // {
        //     foreach (var token in tokens) Console.WriteLine(token);
        //     return;
        // }
        //
        // Parser.Parser parser     = new(tokens);
        // var           statements = parser.Parse();
        //
        // if (HadError) return;
        //
        // if (_command == _COMMANDS[1])
        // {
        //     var expression = parser.ParseExpression();
        //     Console.WriteLine(Printer.Print(expression));
        //     return;
        // }
        //
        // if (_command == _COMMANDS[2])
        // {
        //     var expression = parser.ParseExpression();
        //     Console.WriteLine(Interpreter.Interpret(expression));
        //     
        //     return;
        // }
        //
        // if (_command != _COMMANDS[3]) return;
        //
        // foreach (var statement in statements) Console.WriteLine(Interpreter.Interpret(statement));

        Tokenizer tokenizer = new(_source);
        tokenizer.Scan();
        
        Parser.Parser parser = new(tokenizer.GetTokens());
        switch (_command)
        {
            case "tokenize":
                
                foreach (var token in tokenizer.GetTokens()) Console.WriteLine(token);
                break;
            
            case "parse":
                
                var expression = parser.ParseExpression();
                
                if (HadError) return;
                
                Console.WriteLine(Printer.Print(expression));
                break;
            
            case "evaluate":
                
                var expression1 = parser.ParseExpression();
                
                if (HadError) return;
                
                Console.WriteLine(Interpreter.Interpret(expression1));
                break;
            
            case "run":
                
                var statements = parser.Parse();
                foreach (var statement in statements) Console.WriteLine(Interpreter.Interpret(statement));
                break;
        }
    }
}