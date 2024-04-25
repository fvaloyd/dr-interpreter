using Interpreter;

namespace InterpreterTests;

public class AstTest
{
    [Fact]
    public void TestString()
    {
        Program p = new()
        {
            Statements = new()
            {
                new LetStatement()
                {
                    Token = new Token() { Type = new(Token.LET), Literal = "let" },
                    Name = new Identifier(new Token() { Type = new(Token.IDENT), Literal = "myVar" }, "myVar"),
                    Value = new Identifier(new Token() { Type = new(Token.IDENT), Literal = "anotherVar"}, "anotherVar")
                }
            }
        };

        Assert.Equal("let myVar = anotherVar;", p.String());
    }
}
