
using System.Text;

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
                if (PeekChar() == '=')
                {
                    var ch = Ch;
                    ReadChar();
                    tok = new(new(Token.EQ), string.Concat(Ch, ch));
                }
                else
                {
                    tok = new(new(Token.ASSIGN), Ch);
                }
                break;
            case '"':
                string s = ReadString();
                tok = new(new(Token.STRING), s);
                break;
            case '+':
                tok = new(new(Token.PLUS), Ch);
                break;
            case ';':
                tok = new(new(Token.SEMICOLON), Ch);
                break;
            case '(':
                tok = new(new(Token.LPAREN), Ch);
                break;
            case ')':
                tok = new(new(Token.RPAREN), Ch);
                break;
            case '{':
                tok = new(new(Token.LBRACE), Ch);
                break;
            case '}':
                tok = new(new(Token.RBRACE), Ch);
                break;
            case ',':
                tok = new(new(Token.COMMA), Ch);
                break;
            case '!':
                if (PeekChar() == '=')
                {
                    var ch = Ch;
                    ReadChar();
                    tok = new(new(Token.NOT_EQ), string.Concat(ch, Ch));
                }
                else
                {
                    tok = new(new(Token.BANG), Ch);
                }
                break;
            case '-':
                tok = new(new(Token.MINUS), Ch);
                break;
            case '/':
                tok = new(new(Token.SLASH), Ch);
                break;
            case '*':
                tok = new(new(Token.ASTERISK), Ch);
                break;
            case '<':
                tok = new(new(Token.LT), Ch);
                break;
            case '>':
                tok = new(new(Token.GT), Ch);
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

    private string ReadString()
    {
        int position = ReadPosition;
        ReadChar();
        while (Ch != '"' || Ch == 0)
        {
            ReadChar();
        }
        ReadChar();
        return Input[position..(Position - 1)];
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

    public char PeekChar()
        => ReadPosition >= Input.Length
            ? '\0'
            : Input[ReadPosition];
}
