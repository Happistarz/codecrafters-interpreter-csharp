
using Token;

if (args.Length < 2)
{
    Console.Error.WriteLine("Usage: ./your_program.sh tokenize <filename>");
    Environment.Exit(1);
}

var command = args[0];
var filename = args[1];


var fileContents = File.ReadAllText(filename);

//Uncomment this block to pass the first stage
if (!string.IsNullOrEmpty(fileContents))
{
    switch (command)
    {
        case "tokenize":
        {
            Tokenizer tokenizer = new(fileContents);
            tokenizer.Scan();
        
            foreach (var token in tokenizer.GetTokens())
            {
                Console.WriteLine(token);
            }
        
            Environment.Exit(tokenizer.GetReturnCode());
            break;
        }
        case "parse":
        {
            Tokenizer tokenizer = new(fileContents);
            tokenizer.Scan();
            
            Parser.Parser parser = new(tokenizer.GetTokens());
            parser.Parse();
            break;
        }
        default:
            Console.Error.WriteLine($"Unknown command: {command}");
            Environment.Exit(1);
            break;
    }
} else {
    Console.WriteLine("EOF  null");
    Environment.Exit(0);
}
