namespace Interpreter;

public static class Evaluator
{
    static _Boolean TRUE = new(true);
    static _Boolean FALSE = new(false);
    static _Null NULL = new();

    // ((2 + 3) + 4)
    public static _Object? Eval(Node node)
        => node switch
        {
            Program pr => evalStatements(pr.Statements),
            ExpressionStatement es => Eval(es.Expression),
            IntegerLiteral il => new Integer(il.Value),
            Boolean b => b.Value ? TRUE : FALSE,
            PrefixExpression pe => evalPrefixExpression(pe.Operator, Eval(pe.Right)!),
            InfixExpression ie => evalInfixExpression(ie.Operator, Eval(ie.Left)!, Eval(ie.Right)!),
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

    private static _Object evalInfixExpression(string _operator, _Object left, _Object right)
    {
        if (left.Type() == _Object.INTEGER_OBJ && right.Type() == _Object.INTEGER_OBJ) return evalIntegerInfixExpression(_operator, left, right);
        if (_operator == "==") return nativeBoolToBooleanObject(left == right);
        if (_operator == "!=") return nativeBoolToBooleanObject(left != right);
        return NULL;
    }

    private static _Object evalIntegerInfixExpression(string _operator, _Object left, _Object right)
    {
        var leftVal = ((Integer)left).Value;
        var rightVal = ((Integer)right).Value;
        return _operator switch
        {
            "+" => new Integer(leftVal + rightVal),
            "-" => new Integer(leftVal - rightVal),
            "*" => new Integer(leftVal * rightVal),
            "/" => new Integer(leftVal / rightVal),
            "<" => nativeBoolToBooleanObject(leftVal < rightVal),
            ">" => nativeBoolToBooleanObject(leftVal > rightVal),
            "==" => nativeBoolToBooleanObject(leftVal == rightVal),
            "!=" => nativeBoolToBooleanObject(leftVal != rightVal),
            _ => NULL
        };
    }

    private static _Object nativeBoolToBooleanObject(bool input)
        => input ? TRUE : FALSE;
}
