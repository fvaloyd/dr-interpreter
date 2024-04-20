namespace Interpreter;

public record Parser
{
    public Parser(Lexer lexer)
    {
        Lexer = lexer;
        NextToken();
        NextToken();
    }

    public Token CurrToken { get; set; } = null!;
    public Token PeekToken { get; set; } = null!;
    public Lexer Lexer { get; set; }

    public void NextToken()
    {
        CurrToken = PeekToken;
        PeekToken = Lexer.NextToken();
    }

    public Program ParseProgram()
    {
        return null!;
    }
}
