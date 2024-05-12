namespace Interpreter;

public static class Evaluator
{
    public static _Boolean TRUE = new(true);
    public static _Boolean FALSE = new(false);
    public static _Null NULL = new();

    public static _Object? Eval(Node node)
    {
        switch (node)
        {
            case Program pr:
                return evalProgram(pr);
            case BlockStatement bs:
                return evalBlockStatement(bs);
            case ExpressionStatement es:
                return Eval(es.Expression);
            case ReturnStatement rs:
                var val = Eval(rs.ReturnValue);
                if (isError(val)) return val;
                return new ReturnValue(val!);
            case IntegerLiteral it:
                return new Integer(it.Value);
            case Boolean b:
                return b.Value ? TRUE : FALSE;
            case PrefixExpression pe:
                var right = Eval(pe.Right);
                if (isError(right)) return right;
                return evalPrefixExpression(pe.Operator, right!);
            case InfixExpression ie:
                var left = Eval(ie.Left);
                if (isError(left)) return left;
                var ieRight = Eval(ie.Right);
                if (isError(ieRight)) return ieRight;
                return evalInfixExpression(ie.Operator, left!, ieRight!);
            case IfExpression ie:
                return evalIfExpression(ie);
            default:
                return null;
        }
    }

    static bool isError(_Object? obj)
        => obj is not null && obj.Type() == _Object.ERROR_OBJ;

    static _Object? evalProgram(Program pr)
    {
        _Object? result = null;
        foreach (var stmt in pr.Statements)
        {
            result = Eval(stmt);
            if (result is ReturnValue rv) return rv.Value;
            if (result is Error err) return err;
        }
        return result;
    }

    static _Object? evalBlockStatement(BlockStatement block)
    {
        _Object? result = null;
        foreach (var stmt in block.Statements)
        {
            result = Eval(stmt);
            if (result is not null)
            {
                var rt = result.Type();
                if (rt == _Object.RETURN_VALUE_OBJ || rt == _Object.ERROR_OBJ) return result;
            }
            if (result!.Type() == _Object.RETURN_VALUE_OBJ) return result;
        }
        return result;
    }

    static _Object? evalPrefixExpression(string _operator, _Object right)
        => _operator switch
        {
            "!" => evalBangOperatorExpression(right),
            "-" => evalMinusPrefixOperatorExpression(right),
            _ => new Error($"unknown operator: {_operator}{right.Type()}")
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
        if (right.Type() != _Object.INTEGER_OBJ) return new Error($"unknown operator: -{right.Type()}");
        var value = ((Integer)right).Value;
        return new Integer(-value);
    }

    static _Object evalInfixExpression(string _operator, _Object left, _Object right)
    {
        if (left.Type() == _Object.INTEGER_OBJ && right.Type() == _Object.INTEGER_OBJ) return evalIntegerInfixExpression(_operator, left, right);
        if (_operator == "==") return nativeBoolToBooleanObject(left == right);
        if (_operator == "!=") return nativeBoolToBooleanObject(left != right);
        if (left.Type() != right.Type()) return new Error($"type mismatch: {left.Type()} {_operator} {right.Type()}");
        return new Error($"unknown operator: {left.Type()} {_operator} {right.Type()}");
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
            _ => new Error($"unknown operator: {left.Type()} {_operator} {right.Type()}")
        };
    }

    static _Object nativeBoolToBooleanObject(bool input)
        => input ? TRUE : FALSE;

    static _Object? evalIfExpression(IfExpression ie)
    {
        var condition = Eval(ie.Codition);
        if (isError(condition)) return condition;
        if (isTruthy(condition!)) return Eval(ie.Consequence);
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
