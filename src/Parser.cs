namespace Interpreter;

public record Parser
{
    public Parser(Lexer lexer)
    {
        Lexer = lexer;
        NextToken();
        NextToken();

        RegisterPrefix(new(Token.IDENT), ParseIdentifier);
        RegisterPrefix(new(Token.INT), ParseIntegerLiteral);
        RegisterPrefix(new(Token.MINUS), ParsePrefixExpression);
        RegisterPrefix(new(Token.BANG), ParsePrefixExpression);
    }

    public Token CurrToken { get; set; } = null!;
    public Token PeekToken { get; set; } = null!;
    public Lexer Lexer { get; set; }
    public List<string> Errors { get; set; } = new();
    public Dictionary<TokenType, PrefixParseFn> PrefixParseFns = new();
    public Dictionary<TokenType, InfixParseFn> InfixParseFns = new();

    public void NextToken()
    {
        CurrToken = PeekToken;
        PeekToken = Lexer.NextToken();
    }

    public Program ParseProgram()
    {
        Program p = new();

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
            Token.RETURN => ParseReturnStatement(),
            _ => ParseExpressionStatement()
        };
    }

    public ReturnStatement ParseReturnStatement()
    {
        ReturnStatement rs = new() { Token = CurrToken };
        NextToken();
        while (!CurTokenIs(Token.SEMICOLON))
        {
            NextToken();
        }
        return rs;
    }

    public LetStatement ParseLetStatement()
    {
        LetStatement ls = new() { Token = CurrToken };

        if (!ExpectPeek(Token.IDENT)) return null!;

        ls.Name = new Identifier(CurrToken, CurrToken.Literal);

        if (!ExpectPeek(Token.ASSIGN)) return null!;

        while (!CurTokenIs(Token.SEMICOLON))
        {
            NextToken();
        }

        return ls;
    }

    public ExpressionStatement ParseExpressionStatement()
    {
        var stmt = new ExpressionStatement() { Token = CurrToken };
        stmt.Expression = ParseExpression(Precedence.LOWEST);

        if (PeekTokenIs(Token.SEMICOLON))
        {
            NextToken();
        }
        return stmt;
    }

    public Expression ParseExpression(Precedence pre)
    {
        bool result = PrefixParseFns.TryGetValue(CurrToken.Type, out var prefixFn);
        if (!result || prefixFn is null)
        {
            NoPrefixParseFnError(CurrToken.Type);
            return null!;
        }

        var leftExp = prefixFn();

        return leftExp;
    }

    private void NoPrefixParseFnError(TokenType tt)
    {
        var msg = $"no prefix parse function for {tt.Value} found";
        Errors.Add(msg);
    }

    public Expression ParseIdentifier()
        => new Identifier(CurrToken, CurrToken.Literal);

    public Expression ParseIntegerLiteral()
    {
        var result = Int64.TryParse(CurrToken.Literal, out var value);
        if (!result)
        {
            var msg = $"coult not parse {CurrToken.Literal} as integer";
            Errors.Add(msg);
            return null!;
        }
        return new IntegerLiteral(CurrToken, value);
    }

    public Expression ParsePrefixExpression()
    {
        var exp = new PrefixExpression { Token = CurrToken, Operator = CurrToken.Literal };

        NextToken();

        exp.Right = ParseExpression(Precedence.PREFIX);

        return exp;
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

    public void RegisterPrefix(TokenType tt, PrefixParseFn fn)
    {
        var result = PrefixParseFns.TryAdd(tt, fn);
        if (!result) return;
    }

    public void RegisterInfix(TokenType tt, InfixParseFn fn)
    {
        var result = InfixParseFns.TryAdd(tt, fn);
        if (!result) return;
    }
}

public delegate Expression PrefixParseFn();
public delegate Expression InfixParseFn(Expression exp);
public enum Precedence
{
    LOWEST,
    EQUALS,
    LESSGREATER,
    SUM,
    PRODUCT,
    PREFIX,
    CALL
}
