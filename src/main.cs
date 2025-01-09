
using Token;

if (args.Length < 2)
{
    Console.Error.WriteLine("Usage: ./your_program.sh tokenize <filename>");
    Environment.Exit(1);
}

var command = args[0];
var filename = args[1];

if (command != "tokenize")
{
    Console.Error.WriteLine($"Unknown command: {command}");
    Environment.Exit(1);
}

var fileContents = File.ReadAllText(filename);

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.Error.WriteLine("Logs from your program will appear here!");

//Uncomment this block to pass the first stage
if (!string.IsNullOrEmpty(fileContents))
{
    Tokenizer tokenizer = new(fileContents);
    tokenizer.Scan();
    
    foreach (var token in tokenizer.GetTokens())
    {
        Console.WriteLine(token);
    }
    
    Environment.Exit(tokenizer.GetReturnCode());
}