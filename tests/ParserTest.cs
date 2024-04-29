using Interpreter;
using Xunit.Abstractions;

namespace InterpreterTests;

public class ParserTest
{
    ITestOutputHelper output;

    public ParserTest(ITestOutputHelper output) => this.output = output;

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

        CheckParseErrors(p);

        Assert.NotNull(program);
        Assert.Equal(3, program.Statements.Count);

        string[] expectIdents = new string[] { "x", "y", "foobar" };

        for (int i = 0; i < expectIdents.Length; i++)
        {
            var stmt = program.Statements[i];
            testLetStatement(stmt, expectIdents[i]);
        }
    }

    static void testLetStatement(Statement s, string name)
    {
        Assert.Equal("let", s.TokenLiteral());
        Assert.IsType<LetStatement>(s);
        LetStatement ls = (LetStatement)s;
        Assert.Equal(name, ls.Name.Value);
        Assert.Equal(name, ls.Name.TokenLiteral());
    }

    void CheckParseErrors(Parser p)
    {
        if (p.Errors.Count == 0) return;

        output.WriteLine($"parser has {p.Errors.Count} errors");
        foreach (var err in p.Errors)
        {
            output.WriteLine($"parser error: {err}");
        }
        Assert.Fail("Errors in parser");
    }

    [Fact]
    public void ParseReturnStatements()
    {
        string input = """
            return 5;
            return 10;
            return 993322;
            """;

        Lexer l = Lexer.Create(input);
        Parser p = new(l);

        Program program = p.ParseProgram();

        CheckParseErrors(p);


        Assert.NotNull(program);
        Assert.Equal(3, program.Statements.Count);

        foreach (Statement stmt in program.Statements)
        {
            Assert.IsType<ReturnStatement>(stmt);
            ReturnStatement ts = (ReturnStatement)stmt;
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
        CheckParseErrors(p);

        Assert.Single(pr.Statements);

        ExpressionStatement stmt = (ExpressionStatement)pr.Statements[0];
        Assert.NotNull(stmt);
        Identifier ident = (Identifier)stmt.Expression;
        Assert.NotNull(ident);
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
        CheckParseErrors(p);

        Assert.Single(pr.Statements);
        ExpressionStatement stmt = (ExpressionStatement)pr.Statements[0];
        Assert.NotNull(stmt);
        IntegerLiteral il = (IntegerLiteral)stmt.Expression;
        Assert.NotNull(il);
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
        CheckParseErrors(p);

        Assert.Single(pr.Statements);
        Assert.IsType<ExpressionStatement>(pr.Statements[0]);
        var stmt = (ExpressionStatement)pr.Statements[0];
        Assert.IsType<PrefixExpression>(stmt.Expression);
        var exp = (PrefixExpression)stmt.Expression;

        Assert.Equal(@operator, exp.Operator);
        Assert.True(TestIntegerLiteral(exp.Right, integerValue));
    }

    bool TestIntegerLiteral(Expression exp, Int64 value)
    {
        Assert.IsType<IntegerLiteral>(exp);
        var il = (IntegerLiteral)exp;

        if (il.Value != value)
        {
            output.WriteLine($"il.Value not {value}. got={il.Value}");
            return false;
        }

        if (il.TokenLiteral() != value.ToString())
        {
            output.WriteLine($"il.TokenLiteral() not {value}. got={il.TokenLiteral()}");
            return false;
        }

        return true;
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
        CheckParseErrors(p);

        Assert.Single(pr.Statements);
        var stmt = Assert.IsType<ExpressionStatement>(pr.Statements[0]);
        var exp = Assert.IsType<InfixExpression>(stmt.Expression);
        Assert.True(TestIntegerLiteral(exp.Left, leftValue));
        Assert.Equal(@operator, exp.Operator);
        Assert.True(TestIntegerLiteral(exp.Right, rightValue));
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
        CheckParseErrors(p);

        Assert.Equal(expected, pr.String());
    }
}
