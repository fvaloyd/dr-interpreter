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
    public const string EQ = "==";
    public const string NOT_EQ = "!=";
    public const string BANG = "!";
    public const string MINUS = "-";
    public const string SLASH = "/";
    public const string ASTERISK = "*";
    public const string GT = ">";
    public const string LT = "<";


    // Delimiters
    public const string SEMICOLON = ";";
    public const string COMMA = ",";
    public const string RPAREN = ")";
    public const string LPAREN = "(";
    public const string RBRACE = "}";
    public const string LBRACE = "{";

    // Keywords
    public const string LET = "LET";
    public const string RETURN = "RETURN";
    public const string FUNCTION = "FUNCTION";
    public const string FALSE = "FALSE";
    public const string TRUE = "TRUE";
    public const string IF = "IF";
    public const string ELSE = "ELSE";

    public static Dictionary<string, TokenType> Keywords => new()
    {
        {"let", new(LET)},
        {"fn", new(FUNCTION)},
        {"true", new(TRUE)},
        {"false", new(FALSE)},
        {"return", new(RETURN)},
        {"if", new(IF)},
        {"else", new(ELSE)},
    };

    public static TokenType LookupIdent(string ident)
    {
        Keywords.TryGetValue(ident, out TokenType? tt);
        return tt is null
            ? new(IDENT)
            : tt;
    }
}
