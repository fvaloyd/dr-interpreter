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
    public List<string> Errors { get; set; } = new();

    public void NextToken()
    {
        CurrToken = PeekToken;
        PeekToken = Lexer.NextToken();
    }

    public Program ParseProgram()
    {
        Program p = new(); // ast root

        while (CurrToken.Type.Value != Token.EOF)
        {
            var stmt = ParseStatement();
            if (stmt is not null)
            {
                p.Statements.Add(stmt);
            }
            NextToken();
        }
        return p;
    }

    public Statement? ParseStatement()
    {
        return CurrToken.Type.Value switch
        {
            Token.LET => ParseLetStatement(),
            _ => null
        };
    }

    public LetStatement ParseLetStatement()
    {
        LetStatement ls = new() { Token = CurrToken }; // tt ('let', 'LET') literal 'let'

        if (!ExpectPeek(Token.IDENT)) return null!;

        ls.Name = new Identifier(CurrToken, CurrToken.Literal);

        if (!ExpectPeek(Token.ASSIGN)) return null!;

        while (!CurTokenIs(Token.SEMICOLON))
        {
            NextToken();
        }

        return ls;
    }

    public bool CurTokenIs(string type)
        => CurrToken.Type.Value == type;

    public bool PeekTokenIs(string type)
        => PeekToken.Type.Value == type;

    public bool ExpectPeek(string type)
    {
        if (PeekTokenIs(type))
        {
            NextToken();
            return true;
        }
        else
        {
            PeekError(type);
            return false;
        }
    }

    public void PeekError(string tt)
    {
        string msg = $"expected next token to be {tt}, got {PeekToken.Type.Value} instead";
        Errors.Add(msg);
    }
}
