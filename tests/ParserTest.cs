using Interpreter;

namespace InterpreterTests;

public class ParserTest
{
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

    static void CheckParseErrors(Parser p)
    {
        if (p.Errors.Count == 0) return;

        Console.WriteLine($"parser has {p.Errors.Count} errors");
        foreach (var err in p.Errors)
        {
            Console.WriteLine($"parser error: {err}");
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
}
