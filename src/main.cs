using AST;
using Token;

class Program
{
    public static           bool         HadError        = false;
    public static           bool         HadRuntimeError = false;

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
        Tokenizer tokenizer = new(_source);
        tokenizer.Scan();
        
        Parser.Parser parser = new(tokenizer.GetTokens(), _source);
        switch (_command)
        {
            case "tokenize":
                
                foreach (var token in tokenizer.GetTokens()) Console.WriteLine(token);
                break;
            
            case "parse":
                
                // var expression = parser.ParseExpression();
                var statements0 = parser.Parse();
                
                if (HadError) return;
                
                // Console.WriteLine(Printer.Print(expression));
                Console.WriteLine(Printer.Print(statements0));
                break;
            
            case "evaluate":
                
                var expression1 = parser.ParseExpression();
                
                if (HadError) return;
                
                Console.WriteLine(Interpreter.Interpret(expression1));
                break;
            
            case "run":
                
                var statements = parser.Parse();
                
                if (HadError) return;
                
                Interpreter.Interpret(statements);
                // Console.WriteLine(Printer.Print(statements));
                break;
        }
    }
}