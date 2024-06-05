using Interpreter;

namespace InterpreterTests;

public class LexerTest
{
    [Fact]
    public void TestNewToken()
    {
        string input = """
        let five = 5;
        let ten = 10;

        let add = fn(x, y) {
            x + y;
        };

        let result = add(five, ten);
        !-/*5;
        5 < 10 > 5;

        if (5 < 10) {
            return true;
        } else {
            return false;
        }

        10 == 10;
        10 != 9;
        "foobar"
        "foo bar"
        [1, 2];
        """;

        Token[] tokensExpect = new Token[]
        {
            new(new(Token.LET), "let"),
            new(new(Token.IDENT), "five"),
            new(new(Token.ASSIGN), "="),
            new(new(Token.INT), "5"),
            new(new(Token.SEMICOLON), ";"),
            new(new(Token.LET), "let"),
            new(new(Token.IDENT), "ten"),
            new(new(Token.ASSIGN), "="),
            new(new(Token.INT), "10"),
            new(new(Token.SEMICOLON), ";"),
            new(new(Token.LET), "let"),
            new(new(Token.IDENT), "add"),
            new(new(Token.ASSIGN), "="),
            new(new(Token.FUNCTION), "fn"),
            new(new(Token.LPAREN), "("),
            new(new(Token.IDENT), "x"),
            new(new(Token.COMMA), ","),
            new(new(Token.IDENT), "y"),
            new(new(Token.RPAREN), ")"),
            new(new(Token.LBRACE), "{"),
            new(new(Token.IDENT), "x"),
            new(new(Token.PLUS), "+"),
            new(new(Token.IDENT), "y"),
            new(new(Token.SEMICOLON), ";"),
            new(new(Token.RBRACE), "}"),
            new(new(Token.SEMICOLON), ";"),
            new(new(Token.LET), "let"),
            new(new(Token.IDENT), "result"),
            new(new(Token.ASSIGN), "="),
            new(new(Token.IDENT), "add"),
            new(new(Token.LPAREN), "("),
            new(new(Token.IDENT), "five"),
            new(new(Token.COMMA), ","),
            new(new(Token.IDENT), "ten"),
            new(new(Token.RPAREN), ")"),
            new(new(Token.SEMICOLON), ";"),
            new(new(Token.BANG), "!"),
            new(new(Token.MINUS), "-"),
            new(new(Token.SLASH), "/"),
            new(new(Token.ASTERISK), "*"),
            new(new(Token.INT), "5"),
            new(new(Token.SEMICOLON), ";"),
            new(new(Token.INT), "5"),
            new(new(Token.LT), "<"),
            new(new(Token.INT), "10"),
            new(new(Token.GT), ">"),
            new(new(Token.INT), "5"),
            new(new(Token.SEMICOLON), ";"),
            new(new(Token.IF), "if"),
            new(new(Token.LPAREN), "("),
            new(new(Token.INT), "5"),
            new(new(Token.LT), "<"),
            new(new(Token.INT), "10"),
            new(new(Token.RPAREN), ")"),
            new(new(Token.LBRACE), "{"),
            new(new(Token.RETURN), "return"),
            new(new(Token.TRUE), "true"),
            new(new(Token.SEMICOLON), ";"),
            new(new(Token.RBRACE), "}"),
            new(new(Token.ELSE), "else"),
            new(new(Token.LBRACE), "{"),
            new(new(Token.RETURN), "return"),
            new(new(Token.FALSE), "false"),
            new(new(Token.SEMICOLON), ";"),
            new(new(Token.RBRACE), "}"),
            new(new(Token.INT), "10"),
            new(new(Token.EQ), "=="),
            new(new(Token.INT), "10"),
            new(new(Token.SEMICOLON), ";"),
            new(new(Token.INT), "10"),
            new(new(Token.NOT_EQ), "!="),
            new(new(Token.INT), "9"),
            new(new(Token.SEMICOLON), ";"),
            new(new(Token.STRING), "foobar"),
            new(new(Token.STRING), "foo bar"),
            new(new(Token.LBRACKET), "["),
            new(new(Token.INT), "1"),
            new(new(Token.COMMA), ","),
            new(new(Token.INT), "2"),
            new(new(Token.RBRACKET), "]"),
            new(new(Token.SEMICOLON), ";"),
            new(new(Token.EOF), "")
        };

        Lexer l = Lexer.Create(input);

        foreach (var tt in tokensExpect)
        {
            var tok = l.NextToken();
            Assert.Equal(tt.Type, tok.Type);
            Assert.Equal(tt.Literal, tok.Literal);
        }
    }
}
