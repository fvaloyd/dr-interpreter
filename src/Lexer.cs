namespace Interpreter;

public record Lexer
{
    public string Input { get; set; } = string.Empty;
    public int Position { get; set; }
    public int ReadPosition { get; set; }
    public char Ch { get; set; }

    private Lexer(string input)
        => Input = input;

    public static Lexer Create(string input)
    {
        Lexer l = new(input);
        l.ReadChar();
        return l;
    }

    public void ReadChar()
    {
        if (ReadPosition >= Input.Length)
        {
            Ch = '\0';
        }
        else
        {
            Ch = Input[ReadPosition];
        }
        Position = ReadPosition;
        ReadPosition += 1;
    }

    public Token NextToken()
    {
        Token tok = new();

        SkipWithSpace();

        switch (Ch)
        {
            case '=':
                tok = new(new(Token.ASSIGN), Ch);
                break;
            case '+':
                tok = new(new(Token.PLUS), Ch);
                break;
            case ';':
                tok = new(new(Token.SEMICOLON), Ch);
                break;
            case '\0':
                tok = new(new(Token.EOF), "");
                break;
            default:
                if (IsLetter(Ch))
                {
                    tok.Literal = ReadIdentifier();
                    tok.Type = Token.LookupIdent(tok.Literal);
                    return tok;
                }
                else if (IsDigit(Ch))
                {
                    tok.Literal = ReadNumber();
                    tok.Type = new(Token.INT);
                    return tok;
                }
                else
                {
                    tok = new(new(Token.ILLEGAL), Ch);
                }
                break;
        }

        ReadChar();
        return tok;
    }

    public void SkipWithSpace()
    {
        while (IsWhiteSpace(Ch))
        {
            ReadChar();
        }
    }

    public static bool IsWhiteSpace(char ch)
        => ch == ' ' || ch == '\t' || ch == '\n' || ch == '\r';

    public string ReadIdentifier()
    {
        int position = Position;
        while (IsLetter(Ch))
        {
            ReadChar();
        }
        return Input[position..Position];
    }

    public string ReadNumber()
    {
        int position = Position;
        while (IsDigit(Ch))
        {
            ReadChar();
        }
        return Input[position..Position];
    }

    public static bool IsLetter(char ch)
        => 'a' <= ch && ch <= 'z' || 'A' <= ch && ch <= 'Z' || ch == '_';

    public static bool IsDigit(char ch)
        => '0' <= ch && ch <= '9';
}
