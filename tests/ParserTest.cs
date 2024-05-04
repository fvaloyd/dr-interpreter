using Interpreter;
using Xunit.Abstractions;

namespace InterpreterTests;

public class ParserTest
{
    ITestOutputHelper output;

    public ParserTest(ITestOutputHelper output) => this.output = output;

    void testLetStatement(Statement s, string name)
    {
        Assert.Equal("let", s.TokenLiteral());
        var ls = Assert.IsType<LetStatement>(s);
        Assert.Equal(name, ls.Name.Value);
        Assert.Equal(name, ls.Name.TokenLiteral());
    }

    void checkParseErrors(Parser p)
    {
        if (p.Errors.Count == 0) return;

        output.WriteLine($"parser has {p.Errors.Count} errors");
        foreach (var err in p.Errors)
        {
            output.WriteLine($"parser error: {err}");
        }
        Assert.Fail("Errors in parser");
    }

    void testIdentifier(Expression exp, string value)
    {
        Identifier ident = Assert.IsType<Identifier>(exp);
        Assert.Equal(value, ident.Value);
        Assert.Equal(value, ident.TokenLiteral());
    }

    void testIntegerLiteral(Expression exp, Int64 value)
    {
        IntegerLiteral il = Assert.IsType<IntegerLiteral>(exp);
        Assert.Equal(value, il.Value);
        Assert.Equal(value.ToString(), il.TokenLiteral());
    }

    void testLiteralExpression(Expression exp, Object obj)
    {
        switch (obj)
        {
            case int:
                testIntegerLiteral(exp, (int)obj);
                return;
            case Int64:
                testIntegerLiteral(exp, (int)obj);
                return;
            case string:
                testIdentifier(exp, (string)obj);
                return;
            default:
                Assert.Fail($"type of exp not handle. got={exp}");
                return;
        }
    }

    void testInfixExpression(Expression exp, Object left, string @operator, Object right)
    {
        var opExp = Assert.IsType<InfixExpression>(exp);
        testLiteralExpression(opExp.Left, left);
        Assert.Equal(@operator, opExp.Operator);
        testLiteralExpression(opExp.Right, right);
    }

    [Fact]
    public void TestLetStatement()
    {
        string input = """
            let x = 5;
            let y = 10;
            let foobar = 838383;
            """;

        Lexer l = Lexer.Create(input);
        Parser p = new(l);
        Program program = p.ParseProgram();
        checkParseErrors(p);

        Assert.NotNull(program);
        Assert.Equal(3, program.Statements.Count);

        string[] expectIdents = new string[] { "x", "y", "foobar" };

        for (int i = 0; i < expectIdents.Length; i++)
        {
            var stmt = program.Statements[i];
            testLetStatement(stmt, expectIdents[i]);
        }
    }


    [Fact]
    public void TestReturnStatements()
    {
        string input = """
            return 5;
            return 10;
            return 993322;
            """;

        Lexer l = Lexer.Create(input);
        Parser p = new(l);

        Program program = p.ParseProgram();

        checkParseErrors(p);

        Assert.NotNull(program);
        Assert.Equal(3, program.Statements.Count);

        foreach (Statement stmt in program.Statements)
        {
            var ts = Assert.IsType<ReturnStatement>(stmt);
            Assert.Equal("return", ts.TokenLiteral());
        }
    }

    [Fact]
    public void TestIdentifierExpression()
    {
        string input = "foobar;";
        Lexer l = Lexer.Create(input);
        Parser p = new Parser(l);
        Program pr = p.ParseProgram();
        checkParseErrors(p);

        Assert.Single(pr.Statements);

        var stmt = Assert.IsType<ExpressionStatement>(pr.Statements[0]);
        var ident = Assert.IsType<Identifier>(stmt.Expression);
        Assert.Equal("foobar", ident.Value);
        Assert.Equal("foobar", ident.TokenLiteral());
    }

    [Fact]
    public void TestIntegerExpression()
    {
        string input = "5;";
        Lexer l = Lexer.Create(input);
        Parser p = new Parser(l);
        Program pr = p.ParseProgram();
        checkParseErrors(p);

        Assert.Single(pr.Statements);
        var stmt = Assert.IsType<ExpressionStatement>(pr.Statements[0]);
        var il = Assert.IsType<IntegerLiteral>(stmt.Expression);
        Assert.Equal(5, il.Value);
        Assert.Equal("5", il.TokenLiteral());
    }

    [Theory]
    [InlineData("!5;", "!", 5)]
    [InlineData("-15;", "-", 15)]
    public void TestParsingPrefixExpression(string input, string @operator, Int64 integerValue)
    {
        Lexer l = Lexer.Create(input);
        Parser p = new Parser(l);
        Program pr = p.ParseProgram();
        checkParseErrors(p);

        Assert.Single(pr.Statements);
        var stmt = Assert.IsType<ExpressionStatement>(pr.Statements[0]);
        var exp = Assert.IsType<PrefixExpression>(stmt.Expression);

        Assert.Equal(@operator, exp.Operator);
        testIntegerLiteral(exp.Right, integerValue);
    }


    [Theory]
    [InlineData("5 + 5", 5, "+", 5)]
    [InlineData("5 - 5", 5, "-", 5)]
    [InlineData("5 * 5", 5, "*", 5)]
    [InlineData("5 / 5", 5, "/", 5)]
    [InlineData("5 > 5", 5, ">", 5)]
    [InlineData("5 < 5", 5, "<", 5)]
    [InlineData("5 == 5", 5, "==", 5)]
    [InlineData("5 != 5", 5, "!=", 5)]
    public void TestParsingInfixExpression(string input, Int64 leftValue, string @operator, Int64 rightValue)
    {
        Lexer l = Lexer.Create(input);
        Parser p = new Parser(l);
        Program pr = p.ParseProgram();
        checkParseErrors(p);

        Assert.Single(pr.Statements);
        var stmt = Assert.IsType<ExpressionStatement>(pr.Statements[0]);
        var exp = Assert.IsType<InfixExpression>(stmt.Expression);
        testIntegerLiteral(exp.Left, leftValue);
        Assert.Equal(@operator, exp.Operator);
        testIntegerLiteral(exp.Right, rightValue);
    }

    [Theory]
    [InlineData("-a * b", "((-a) * b)")]
    [InlineData("!-a", "(!(-a))")]
    [InlineData("a + b + c", "((a + b) + c)")]
    [InlineData("a + b - c", "((a + b) - c)")]
    [InlineData("a * b * c", "((a * b) * c)")]
    [InlineData("a * b / c", "((a * b) / c)")]
    [InlineData("a + b / c", "(a + (b / c))")]
    [InlineData("a + b * c + d / e - f", "(((a + (b * c)) + (d / e)) - f)")]
    [InlineData("3 + 4; -5 * 5", "(3 + 4)((-5) * 5)")]
    [InlineData("5 > 4 == 3 < 4", "((5 > 4) == (3 < 4))")]
    [InlineData("5 < 4 != 3 > 4", "((5 < 4) != (3 > 4))")]
    [InlineData("3 + 4 * 5 == 3 * 1 + 4 * 5", "((3 + (4 * 5)) == ((3 * 1) + (4 * 5)))")]
    public void TestOperatorPrecedenceParsing(string input, string expected)
    {
        Lexer l = Lexer.Create(input);
        Parser p = new Parser(l);
        Program pr = p.ParseProgram();
        checkParseErrors(p);

        Assert.Equal(expected, pr.String());
    }

}
