using Interpreter;

namespace InterpreterTests;

public class LexerTest
{
    [Fact]
    public void TestNewToken()
    {
        string input = """
        let x = 5 + 6;
        """;

        Token[] tokensExpect = new Token[]
        {
            new(new(Token.LET), "let"),
            new(new(Token.IDENT), "x"),
            new(new(Token.ASSIGN), "="),
            new(new(Token.INT), "5"),
            new(new(Token.PLUS), "+"),
            new(new(Token.INT), "6"),
            new(new(Token.SEMICOLON), ";"),
            new(new(Token.EOF), ""),
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
