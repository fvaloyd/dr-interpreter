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
    [InlineData("5 + 5 + 5 + 5 - 10", 10)]
    [InlineData("2 * 2 * 2 * 2 * 2", 32)]
    [InlineData("-50 + 100 + -50", 0)]
    [InlineData("5 * 2 + 10", 20)]
    [InlineData("5 + 2 * 10", 25)]
    [InlineData("20 + 2 * -10", 0)]
    [InlineData("50 / 2 * 2 + 10", 60)]
    [InlineData("2 * (5 + 10)", 30)]
    [InlineData("3 * 3 * 3 + 10", 37)]
    [InlineData("3 * (3 * 3) + 10", 37)]
    [InlineData("(5 + 10 * 2 + 15 / 3) * 2 + -10", 50)]
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
    [InlineData("1 < 2", true)]
    [InlineData("1 > 2", false)]
    [InlineData("1 < 1", false)]
    [InlineData("1 > 1", false)]
    [InlineData("1 == 1", true)]
    [InlineData("1 != 1", false)]
    [InlineData("1 == 2", false)]
    [InlineData("1 != 2", true)]
    [InlineData("true == true", true)]
    [InlineData("false == false", true)]
    [InlineData("true == false", false)]
    [InlineData("true != false", true)]
    [InlineData("false != true", true)]
    [InlineData("(1 < 2) == true", true)]
    [InlineData("(1 < 2) == false", false)]
    [InlineData("(1 > 2) == true", false)]
    [InlineData("(1 > 2) == false", true)]
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

    [Theory]
    [InlineData("if (true) { 10 }", 10)]
    [InlineData("if (false) { 10 }", null)]
    [InlineData("if (1) { 10 }", 10)]
    [InlineData("if (1 < 2) { 10 }", 10)]
    [InlineData("if (1 > 2) { 10 }", null)]
    [InlineData("if (1 > 2) { 10 } else { 20 }", 20)]
    [InlineData("if (1 < 2) { 10 } else { 20 }", 10)]
    public void TestIfElseExpression(string input, Object expected)
    {
        var evaluated = testEval(input);
        Assert.NotNull(evaluated);
        if (expected is not null)
        {
            testIntegerObject(evaluated, (int)expected);
        }
        else
        {
            Assert.Equal(Evaluator.NULL, evaluated);
        }
    }

    [Theory]
    [InlineData("return 10;", 10)]
    [InlineData("return 10; 9;", 10)]
    [InlineData("return 2 * 5; 9;", 10)]
    [InlineData("9; return 2 * 5; 9;", 10)]
    [InlineData("""
            if (10 > 1) {
                if (10 > 1) {
                    return 10;
                }
                return 1;
            }
            """, 10)]
    public void TestReturnStatement(string input, Int64 expected)
    {
        var evaluated = testEval(input);
        Assert.NotNull(evaluated);
        testIntegerObject(evaluated, expected);
    }

    [Theory]
    [InlineData("5 + true", "type mismatch: INTEGER + BOOLEAN")]
    [InlineData("5 + true; 5", "type mismatch: INTEGER + BOOLEAN")]
    [InlineData("-true", "unknown operator: -BOOLEAN")]
    [InlineData("true + false", "unknown operator: BOOLEAN + BOOLEAN")]
    [InlineData("5; true + false; 5", "unknown operator: BOOLEAN + BOOLEAN")]
    [InlineData("if (10 > 1) { true + false; }", "unknown operator: BOOLEAN + BOOLEAN")]
    [InlineData("""
            if (10 > 1) {
                if (10 > 1) {
                    return true + false;
                }
                return 1;
            }
            """, "unknown operator: BOOLEAN + BOOLEAN")]
    [InlineData("foobar", "identifier not found foobar")]
    public void TestErrorHandling(string input, string expectedMessage)
    {
        var evaluated = testEval(input);
        var errObj = Assert.IsType<Error>(evaluated);
        Assert.Equal(expectedMessage, errObj.Message);
    }

    [Theory]
    [InlineData("let a = 5; a;", 5)]
    [InlineData("let a = 5 * 5; a;", 25)]
    [InlineData("let a = 5; let b = a; b;", 5)]
    [InlineData("let a = 5; let b = a; let c = a + b + 5;", 15)]
    public void TestLetStatements(string input, int expected)
    {
        testIntegerObject(testEval(input)!, expected);
    }
}
