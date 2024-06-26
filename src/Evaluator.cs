namespace Interpreter;

public static class Evaluator
{
    public static _Boolean TRUE = new(true);
    public static _Boolean FALSE = new(false);
    public static _Null NULL = new();

    public static _Object? Eval(Node node, Environment env)
    {
        switch (node)
        {
            case Program pr:
                return evalProgram(pr, env);
            case BlockStatement bs:
                return evalBlockStatement(bs, env);
            case ExpressionStatement es:
                return Eval(es.Expression, env);
            case ReturnStatement rs:
                {
                    var val = Eval(rs.ReturnValue, env);
                    if (isError(val)) return val;
                    return new ReturnValue(val!);
                }
            case IntegerLiteral it:
                return new Integer(it.Value);
            case Boolean b:
                return b.Value ? TRUE : FALSE;
            case StringLiteral sl:
                return new _String(sl.Value);
            case PrefixExpression pe:
                {
                    var right = Eval(pe.Right, env);
                    if (isError(right)) return right;
                    return evalPrefixExpression(pe.Operator, right!);
                }
            case ArrayLiteral al:
                {
                    var elements = evalExpressions(al.Elements, env);
                    if (elements.Count == 1 && isError(elements[0])) return elements[0];
                    return new _Array { Elements = elements.ToArray() };
                }
            case InfixExpression ie:
                {
                    var left = Eval(ie.Left, env);
                    if (isError(left)) return left;
                    var right = Eval(ie.Right, env);
                    if (isError(right)) return right;
                    return evalInfixExpression(ie.Operator, left!, right!);
                }
            case IfExpression ie:
                return evalIfExpression(ie, env);
            case LetStatement ls:
                {
                    var val = Eval(ls.Value, env);
                    if (isError(val)) return val;
                    env.Set(ls.Name.Value, val!);
                    break;
                }
            case Identifier i:
                return evalIdentifier(i, env);
            case FunctionLiteral fl:
                return new Function(fl.Parameters, fl.Body, env);
            case CallExpression ce:
                {
                    var function = Eval(ce.Function, env);
                    if (isError(function) || function is null) return function;
                    var args = evalExpressions(ce.Arguments, env);
                    if (args.Count == 1 && isError(args[0])) return args[0];
                    return applyFunction(function, args);
                }
            default:
                return null;
        }
        return null;
    }

    static _Object applyFunction(_Object func, List<_Object> args)
    {
        switch (func)
        {
            case Function f:
                Function function = (Function)func;
                var extendedEnv = extendFunctionEvn(function, args);
                var evaluated = Eval(function.Body, extendedEnv);
                return unwrapReturnValue(evaluated!);
            case Builtin fn:
                return fn.Fn(args.ToArray());
            default:
                return new Error($"not a function: {func.Type()}");
        }
    }

    static Environment extendFunctionEvn(Function func, List<_Object> args)
    {
        var env = Environment.NewEnclosedEnvironment(func.env);
        for (int i = 0; i < func.Parameters.Count; i++)
        {

        }
        foreach (var param in func.Parameters)
        {
            env.Set(param.Value, args[0]);
        }
        return env;
    }

    static _Object unwrapReturnValue(_Object obj)
    {
        ReturnValue returnValue = null!;
        try
        {
            returnValue = (ReturnValue)obj;
            return returnValue;
        }
        catch (Exception)
        {
            return obj;
        }
    }

    static List<_Object> evalExpressions(List<Expression> exps, Environment env)
    {
        List<_Object> result = [];
        foreach (var e in exps)
        {
            var evaluated = Eval(e, env);
            if (isError(evaluated)) return [evaluated!];
            result.Add(evaluated!);
        }
        return result;
    }

    static _Object evalIdentifier(Identifier i, Environment env)
    {
        env.Get(i.Value, out var obj);
        if (obj is not null) return obj;

        _Object.Builtins.TryGetValue(i.Value, out var builtin);
        if (builtin is not null) return builtin;

        return new Error($"identifier not found: {i.Value}");
    }

    static bool isError(_Object? obj)
        => obj is not null && obj.Type() == _Object.ERROR_OBJ;

    static _Object? evalProgram(Program pr, Environment env)
    {
        _Object? result = null;
        foreach (var stmt in pr.Statements)
        {
            result = Eval(stmt, env);
            if (result is ReturnValue rv) return rv.Value;
            if (result is Error err) return err;
        }
        return result;
    }

    static _Object? evalBlockStatement(BlockStatement block, Environment env)
    {
        _Object? result = null;
        foreach (var stmt in block.Statements)
        {
            result = Eval(stmt, env);
            if (result is not null)
            {
                var rt = result.Type();
                if (rt == _Object.RETURN_VALUE_OBJ || rt == _Object.ERROR_OBJ) return result;
            }
            if (result is not null && result!.Type() == _Object.RETURN_VALUE_OBJ) return result;
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
        if (left.Type() == _Object.STRING_OBJ && right.Type() == _Object.STRING_OBJ) return evalStringInfixExpression(_operator, left, right);
        return new Error($"unknown operator: {left.Type()} {_operator} {right.Type()}");
    }
    static _Object evalStringInfixExpression(string _operator, _Object left, _Object right)
    {
        if (_operator != "+") return new Error($"unknown operator: {left.Type()} {_operator} {right.Type()}");

        var leftVal = ((_String)left).Value;
        var rightVal = ((_String)right).Value;
        return new _String(leftVal + rightVal);
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

    static _Object? evalIfExpression(IfExpression ie, Environment env)
    {
        var condition = Eval(ie.Codition, env);
        if (isError(condition)) return condition;
        if (isTruthy(condition!)) return Eval(ie.Consequence, env);
        else if (ie.Alternative is not null) return Eval(ie.Alternative, env);
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
