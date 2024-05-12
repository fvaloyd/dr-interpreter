namespace Interpreter;

public static class Evaluator
{
    public static _Boolean TRUE = new(true);
    public static _Boolean FALSE = new(false);
    public static _Null NULL = new();

    public static _Object? Eval(Node node)
        => node switch
        {
            Program pr => evalProgram(pr),
            BlockStatement bs => evalBlockStatement(bs),
            ExpressionStatement es => Eval(es.Expression),
            ReturnStatement rs => new ReturnValue(Eval(rs.ReturnValue)!),
            IntegerLiteral il => new Integer(il.Value),
            Boolean b => b.Value ? TRUE : FALSE,
            PrefixExpression pe => evalPrefixExpression(pe.Operator, Eval(pe.Right)!),
            InfixExpression ie => evalInfixExpression(ie.Operator, Eval(ie.Left)!, Eval(ie.Right)!),
            IfExpression ie => evalIfExpression(ie),
            _ => null
        };

    static _Object? evalProgram(Program pr)
    {
        _Object? result = null;
        foreach (var stmt in pr.Statements)
        {
            result = Eval(stmt);
            if (result is ReturnValue rv)
            {
                return rv.Value;
            }
        }
        return result;
    }

    static _Object? evalBlockStatement(BlockStatement block)
    {
        _Object? result = null;
        foreach (var stmt in block.Statements)
        {
            result = Eval(stmt);
            if (result!.Type() == _Object.RETURN_VALUE_OBJ) return result;
        }
        return result;
    }

    static _Object? evalPrefixExpression(string _operator, _Object right)
        => _operator switch
        {
            "!" => evalBangOperatorExpression(right),
            "-" => evalMinusPrefixOperatorExpression(right),
            _ => null
        };

    static _Object evalBangOperatorExpression(_Object right)
    {
        if (right.Equals(TRUE)) return FALSE;
        if (right.Equals(FALSE)) return TRUE;
        if (right.Equals(NULL)) return TRUE;
        return FALSE;
    }

    static _Object evalMinusPrefixOperatorExpression(_Object right)
    {
        if (right.Type() != _Object.INTEGER_OBJ) return NULL;
        var value = ((Integer)right).Value;
        return new Integer(-value);
    }

    static _Object evalInfixExpression(string _operator, _Object left, _Object right)
    {
        if (left.Type() == _Object.INTEGER_OBJ && right.Type() == _Object.INTEGER_OBJ) return evalIntegerInfixExpression(_operator, left, right);
        if (_operator == "==") return nativeBoolToBooleanObject(left == right);
        if (_operator == "!=") return nativeBoolToBooleanObject(left != right);
        return NULL;
    }

    static _Object evalIntegerInfixExpression(string _operator, _Object left, _Object right)
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

    static _Object nativeBoolToBooleanObject(bool input)
        => input ? TRUE : FALSE;

    static _Object? evalIfExpression(IfExpression ie)
    {
        var condition = Eval(ie.Codition);
        if (condition is null) return NULL;
        if (isTruthy(condition)) return Eval(ie.Consequence);
        else if (ie.Alternative is not null) return Eval(ie.Alternative);
        else return NULL;
    }

    static bool isTruthy(_Object obj)
    {
        if (obj.Equals(NULL)) return false;
        if (obj.Equals(TRUE)) return true;
        if (obj.Equals(FALSE)) return false;
        return true;
    }
}
