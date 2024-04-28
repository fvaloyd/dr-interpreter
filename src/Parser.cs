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

        RegisterInfix(new(Token.PLUS), ParseInfixExpression);
        RegisterInfix(new(Token.MINUS), ParseInfixExpression);
        RegisterInfix(new(Token.SLASH), ParseInfixExpression);
        RegisterInfix(new(Token.ASTERISK), ParseInfixExpression);
        RegisterInfix(new(Token.EQ), ParseInfixExpression);
        RegisterInfix(new(Token.NOT_EQ), ParseInfixExpression);
        RegisterInfix(new(Token.LT), ParseInfixExpression);
        RegisterInfix(new(Token.GT), ParseInfixExpression);
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

        while (!PeekTokenIs(Token.SEMICOLON) && pre < PeekPrecedence())
        {
            bool resultInfix = InfixParseFns.TryGetValue(PeekToken.Type, out var infixFn);
            if (!resultInfix || infixFn is null)
            {
                return leftExp;
            }
            NextToken();

            leftExp = infixFn(leftExp);
        }

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

    public Expression ParseInfixExpression(Expression left)
    {
        var exp = new InfixExpression { Token = CurrToken, Operator = CurrToken.Literal, Left = left };

        var precedence = CurrPrecedence();
        NextToken();
        exp.Right = ParseExpression(precedence);
        return exp;
    }

    public static Dictionary<TokenType, Precedence> Precedences => new()
    {
        {new(Token.EQ), Precedence.EQUALS},
        {new(Token.NOT_EQ), Precedence.EQUALS},
        {new(Token.LT), Precedence.LESSGREATER},
        {new(Token.GT), Precedence.LESSGREATER},
        {new(Token.PLUS), Precedence.SUM},
        {new(Token.MINUS), Precedence.SUM},
        {new(Token.SLASH), Precedence.PRODUCT},
        {new(Token.ASTERISK), Precedence.PRODUCT},
    };

    public Precedence PeekPrecedence()
    {
        var result = Precedences.TryGetValue(PeekToken.Type, out var precedence);
        return result
            ? precedence
            : Precedence.LOWEST;
    }

    public Precedence CurrPrecedence()
    {
        var result = Precedences.TryGetValue(CurrToken.Type, out var precedence);
        return result
            ? precedence
            : Precedence.LOWEST;
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
