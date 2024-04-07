namespace Interpreter;

public record Repl
{
    public static void Start()
    {
        Console.WriteLine("Type in commands");
        Console.Write(">> ");
        string line = Console.ReadLine() ?? throw new Exception();

        var l = Lexer.Create(line);

        var tok = l.NextToken();
        while (tok.Type.Value != Token.EOF)
        {
            Console.WriteLine(tok);
            tok = l.NextToken();
        }
    }
}
