namespace Interpreter;

public record TokenType(string Value);

public record Token
{
    public TokenType Type { get; set; } = null!;
    public string Literal { get; set; } = string.Empty;

    public Token(TokenType type, string literal)
        => (Type, Literal) = (type, literal);

    public Token(TokenType type, char literal)
        => (Type, Literal) = (type, literal.ToString());

    public Token() { }

    public const string ILLEGAL = "ILLEGAL";
    public const string EOF = "EOF";

    // Identifiers
    public const string IDENT = "IDENT";
    public const string INT = "INT";

    // Operators
    public const string ASSIGN = "=";
    public const string PLUS = "+";

    // Delimiters
    public const string SEMICOLON = ";";

    // Keywords
    public const string LET = "LET";

    public static Dictionary<string, TokenType> Keywords => new()
    {
        {"let", new(LET)},
    };

    public static TokenType LookupIdent(string ident)
    {
        Keywords.TryGetValue(ident, out TokenType? tt);
        return tt is null
            ? new(IDENT)
            : tt;
    }
}
