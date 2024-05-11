using Interpreter;

namespace InterpreterTests;

public class EvaluatorTest
{
    private _Object? testEval(string input)
    {
        Lexer l = Lexer.Create(input);
        Parser p = new Parser(l);
        Program pr = p.ParseProgram();

        return Evaluator.Eval(pr);
    }

    [Theory]
    [InlineData("5", 5)]
    [InlineData("10", 10)]
    [InlineData("-5", -5)]
    [InlineData("-10", -10)]
    public void TestEvalIntegerExpression(string input, Int64 expected)
    {
        var evaluated = testEval(input);
        Assert.NotNull(evaluated);
        testIntegerObject(evaluated, expected);
    }

    private void testIntegerObject(_Object obj, Int64 expected)
    {
        var integer = Assert.IsType<Integer>(obj);
        Assert.Equal(expected, integer.Value);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void TestEvalBooleanExpression(string input, bool expected)
    {
        var evaluated = testEval(input);
        Assert.NotNull(evaluated);
        testBooleanObject(evaluated, expected);
    }

    private void testBooleanObject(_Object obj, bool expected)
    {
        var boolean = Assert.IsType<_Boolean>(obj);
        Assert.Equal(expected, boolean.Value);
    }

    [Theory]
    [InlineData("!true", false)]
    [InlineData("!false", true)]
    [InlineData("!5", false)]
    [InlineData("!!true", true)]
    [InlineData("!!false", false)]
    [InlineData("!!5", true)]
    public void TestBangOperator(string input, bool expected)
    {
        var evaluated = testEval(input);
        Assert.NotNull(evaluated);
        testBooleanObject(evaluated, expected);
    }
}
