using Interpreter;
using Xunit.Abstractions;
using Boolean = Interpreter.Boolean;

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

    void testBooleanLiteral(Expression exp, bool value)
    {
        Boolean bo = Assert.IsType<Boolean>(exp);
        Assert.Equal(value, bo.Value);
        Assert.Equal(value.ToString().ToLower(), bo.TokenLiteral());
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
            case bool:
                testBooleanLiteral(exp, (bool)obj);
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
    [InlineData("!true", "!", true)]
    [InlineData("!false;", "!", false)]
    public void TestParsingPrefixExpression(string input, string @operator, object integerValue)
    {
        Lexer l = Lexer.Create(input);
        Parser p = new Parser(l);
        Program pr = p.ParseProgram();
        checkParseErrors(p);

        Assert.Single(pr.Statements);
        var stmt = Assert.IsType<ExpressionStatement>(pr.Statements[0]);
        var exp = Assert.IsType<PrefixExpression>(stmt.Expression);

        Assert.Equal(@operator, exp.Operator);
        testLiteralExpression(exp.Right, integerValue);
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
    [InlineData("true == true", true, "==", true)]
    [InlineData("true != false", true, "!=", false)]
    [InlineData("false == false", false, "==", false)]
    public void TestParsingInfixExpression(string input, object leftValue, string @operator, object rightValue)
    {
        Lexer l = Lexer.Create(input);
        Parser p = new Parser(l);
        Program pr = p.ParseProgram();
        checkParseErrors(p);

        Assert.Single(pr.Statements);
        var stmt = Assert.IsType<ExpressionStatement>(pr.Statements[0]);
        var exp = Assert.IsType<InfixExpression>(stmt.Expression);
        testInfixExpression(exp, leftValue, @operator, rightValue);
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
    [InlineData("true", "true")]
    [InlineData("false", "false")]
    [InlineData("3 > 5 == false", "((3 > 5) == false)")]
    [InlineData("3 > 5 == true", "((3 > 5) == true)")]
    [InlineData("1 + (2 + 3) + 4", "((1 + (2 + 3)) + 4)")]
    [InlineData("(5 + 5) * 2", "((5 + 5) * 2)")]
    [InlineData("2 / (5 + 5)", "(2 / (5 + 5))")]
    [InlineData("-(5 + 5)", "(-(5 + 5))")]
    [InlineData("!(true == true)", "(!(true == true))")]
    public void TestOperatorPrecedenceParsing(string input, string expected)
    {
        Lexer l = Lexer.Create(input);
        Parser p = new Parser(l);
        Program pr = p.ParseProgram();
        checkParseErrors(p);

        Assert.Equal(expected, pr.String());
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void TestBooleanExpression(string input, bool expectedBoolean)
    {

        Lexer l = Lexer.Create(input);
        Parser p = new Parser(l);
        Program pr = p.ParseProgram();
        checkParseErrors(p);

        Assert.Single(pr.Statements);
        var stmt = Assert.IsType<ExpressionStatement>(pr.Statements[0]);
        var boolean = Assert.IsType<Boolean>(stmt.Expression);
        Assert.Equal(expectedBoolean, boolean.Value);
    }

    [Fact]
    public void TestIfExpression()
    {
        string input = "if (x < y) { x }";
        Lexer l = Lexer.Create(input);
        Parser p = new Parser(l);
        Program pr = p.ParseProgram();
        checkParseErrors(p);

        Assert.Single(pr.Statements);
        var stmt = Assert.IsType<ExpressionStatement>(pr.Statements[0]);
        var exp = Assert.IsType<IfExpression>(stmt.Expression);
        testInfixExpression(exp.Codition, "x", "<", "y");
        Assert.Single(exp.Consequence.Statements);
        var consequence = Assert.IsType<ExpressionStatement>(exp.Consequence.Statements[0]);
        testIdentifier(consequence.Expression, "x");
        Assert.Null(exp.Alternative);
    }

    [Fact]
    public void TestIfElseExpression()
    {
        string input = "if (x < y) { x } else { y }";
        Lexer l = Lexer.Create(input);
        Parser p = new Parser(l);
        Program pr = p.ParseProgram();
        checkParseErrors(p);

        Assert.Single(pr.Statements);
        var stmt = Assert.IsType<ExpressionStatement>(pr.Statements[0]);
        var exp = Assert.IsType<IfExpression>(stmt.Expression);
        testInfixExpression(exp.Codition, "x", "<", "y");

        Assert.Single(exp.Consequence.Statements);
        var consequence = Assert.IsType<ExpressionStatement>(exp.Consequence.Statements[0]);
        testIdentifier(consequence.Expression, "x");

        Assert.NotNull(exp.Alternative);
        Assert.Single(exp.Alternative.Statements);
        var alternative = Assert.IsType<ExpressionStatement>(exp.Alternative.Statements[0]);
        testIdentifier(alternative.Expression, "y");
    }
}
