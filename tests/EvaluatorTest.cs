using Interpreter;

namespace InterpreterTests;

public class EvaluatorTest
{
    private _Object? testEval(string input)
    {
        Lexer l = Lexer.Create(input);
        Parser p = new Parser(l);
        Program pr = p.ParseProgram();

        return Evaluator.Eval(pr, new());
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

    private void testIntegerObject(_Object obj, Int64? expected)
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
    [InlineData("""
            let f = fn(x) {
                return x;
                x + 10;
            };
            f(10);
            """, 10)]
    [InlineData("""
            let f = fn(x) {
                let result = x + 10;
                return result;
                return 10;
            };
            f(10);
            """, 20)]
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
    [InlineData("foobar", "identifier not found: foobar")]
    [InlineData("\"Hello\" - \"World\"", "unknown operator: STRING - STRING")]
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
    [InlineData("let a = 5; let b = a; let c = a + b + 5; c;", 15)]
    public void TestLetStatements(string input, int expected)
    {
        testIntegerObject(testEval(input)!, expected);
    }

    [Fact]
    public void TestFunctionObject()
    {
        string input = "fn(x) { x + 2; };";
        var evaluated = testEval(input);
        var fn = Assert.IsType<Function>(evaluated);
        Assert.Equal("x", fn.Parameters.First().String());
        Assert.Equal("(x + 2)", fn.Body.String());
    }

    [Theory]
    [InlineData("let identity = fn(x) { x; }; identity(5);", 5)]
    [InlineData("let identity = fn(x) { return x; }; identity(5);", 5)]
    [InlineData("let double = fn(x) { x * 2; }; double(5);", 10)]
    [InlineData("let add = fn(x, y) { x + y; }; add(5, 5);", 10)]
    [InlineData("let add = fn(x, y) { x + y; }; add(5 + 5, add(5, 5));", 20)]
    public void TestFunctionApplication(string input, int expected)
    {
        testIntegerObject(testEval(input)!, expected);
    }

    [Fact]
    public void TestStringLiteral()
    {
        var input = "\"Hello world!\"";
        var evaluated = testEval(input);
        var str = Assert.IsType<_String>(evaluated);
        Assert.Equal("Hello world!", str.Value);
    }

    [Fact]
    public void TestStringConcatenation()
    {
        var input = """
            "Hello " + "World!"
            """;
        var evaluated = testEval(input);
        var str = Assert.IsType<_String>(evaluated);
        Assert.Equal("Hello World!", str.Value);
    }

    [Theory]
    [InlineData("""len("")""", 0)]
    [InlineData("""len("four")""", 4)]
    [InlineData("""len("hello world")""", 11)]
    [InlineData("""len(1)""", "argument to `len` not supported, got INTEGER")]
    [InlineData("""len("one", "two")""", "wrong number of arguments. got=2, want=1")]
    public void TestBuiltinFunctions(string input, Object expected)
    {
        var evaluated = testEval(input);

        switch (expected)
        {
            case int i:
                testIntegerObject(evaluated!, i);
                break;
            case string s:
                var errObj = Assert.IsType<Error>(evaluated);
                Assert.Equal(s, errObj.Message);
                break;
        }
    }

    [Fact]
    public void TestArrayLiterals()
    {
        var input = "[1, 2 * 2, 3 + 3]";
        var evaluated = testEval(input);
        var result = Assert.IsType<_Array>(evaluated);
        Assert.Equal(3, result.Elements.Length);
        testIntegerObject(result.Elements[0], 1);
        testIntegerObject(result.Elements[1], 4);
        testIntegerObject(result.Elements[2], 6);
    }

    [Theory]
    [InlineData("[1, 2, 3][0]", 1)]
    [InlineData("[1, 2, 3][1]", 2)]
    [InlineData("[1, 2, 3][2]", 3)]
    [InlineData("let i = 0; [1][i]", 1)]
    [InlineData("[1, 2, 3][1 + 1]", 3)]
    [InlineData("let myArray = [1, 2, 3]; myArray[2]", 3)]
    [InlineData("let myArray = [1, 2, 3]; myArray[0] + myArray[1] + myArray[2];", 6)]
    [InlineData("let myArray = [1, 2, 3]; let i = myArray[0]; myArray[i]", 2)]
    [InlineData("[1, 2, 3][3]", null)]
    [InlineData("[1, 2, 3][-1]", null)]
    public void TestArrayIndexExpressions(string input, int? expected)
    {
        var evaluated = testEval(input);
        if (expected is null)
        {
            testNullObject(evaluated);
        }
        else
        {
            testIntegerObject(evaluated!, expected);
        }
    }

    private void testNullObject(_Object? obj)
    {
        Assert.IsType<_Null>(obj);
    }
}
