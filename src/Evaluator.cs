namespace Interpreter;

public static class Evaluator
{
    static _Boolean TRUE = new(true);
    static _Boolean FALSE = new(false);
    static _Null NULL = new();

    public static _Object? Eval(Node node)
        => node switch
        {
            Program pr => evalStatements(pr.Statements),
            ExpressionStatement es => Eval(es.Expression),
            IntegerLiteral il => new Integer(il.Value),
            Boolean b => b.Value ? TRUE : FALSE,
            PrefixExpression pe => evalPrefixExpression(pe.Operator, Eval(pe.Right)!),
            _ => null
        };

    public static _Object? evalStatements(List<Statement> stmts)
    {
        _Object? result = null;
        foreach (var stmt in stmts)
        {
            result = Eval(stmt);
        }
        return result;
    }

    private static _Object? evalPrefixExpression(string _operator, _Object right)
        => _operator switch
        {
            "!" => evalBangOperatorExpression(right),
            "-" => evalMinusPrefixOperatorExpression(right),
            _ => null
        };

    private static _Object evalBangOperatorExpression(_Object right)
    {
        if (right.Equals(TRUE)) return FALSE;
        if (right.Equals(FALSE)) return TRUE;
        if (right.Equals(NULL)) return TRUE;
        return FALSE;
    }

    private static _Object evalMinusPrefixOperatorExpression(_Object right)
    {
        if (right.Type() != _Object.INTEGER_OBJ) return NULL;
        var value = ((Integer)right).Value;
        return new Integer(-value);
    }
}
