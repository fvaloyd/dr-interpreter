namespace Interpreter;

public class Evaluator
{
    public static _Object? Eval(Node node)
        => node switch
        {
            IntegerLiteral il => new Integer(il.Value),
            _ => null
        };
}
