namespace Interpreter;

using System.Text;
using ObjectType = string;

public abstract record _Object
{
    public const string INTEGER_OBJ = "INTEGER";
    public const string BOOLEAN_OBJ = "BOOLEAN";
    public const string NULL_OBJ = "NULL";
    public const string RETURN_VALUE_OBJ = "RETURN_VALUE";
    public const string ERROR_OBJ = "ERROR";
    public const string FUNCTION_OBJ = "FUNCTION";
    public const string STRING_OBJ = "STRING";
    public const string BUILTIN = "BUILTIN";
    public const string ARRAY = "ARRAY";

    public abstract ObjectType Type();
    public abstract string Inspect();

    public static Dictionary<string, Builtin> Builtins => new()
    {
        { "len", new((params _Object[] args) => {
                if (args.Length != 1)
                {
                    return new Error($"wrong number of arguments. got={args.Length}, want=1");
                }
                switch (args[0])
                {
                    case _String s:
                        return new Integer(s.Value.Length);
                    default:
                        return new Error($"argument to `len` not supported, got {args[0].Type()}");
                }
                })},
    };
}

public record Integer(Int64 Value) : _Object
{
    public override string Inspect()
        => Value.ToString();

    public override ObjectType Type()
        => INTEGER_OBJ;
}

public record _Boolean(bool Value) : _Object
{
    public override string Inspect()
        => Value.ToString().ToLower();

    public override ObjectType Type()
        => BOOLEAN_OBJ;
}

public record _Null : _Object
{
    public override string Inspect()
        => "null";

    public override ObjectType Type()
        => NULL_OBJ;
}

public record ReturnValue(_Object Value) : _Object
{
    public override string Inspect()
        => Value.Inspect();

    public override string Type()
        => _Object.RETURN_VALUE_OBJ;
}

public record Error(string Message) : _Object
{
    public override string Inspect()
        => $"ERROR: {Message}";

    public override string Type()
        => _Object.ERROR_OBJ;
}

public record _String(string Value) : _Object
{
    public override string Inspect()
        => Value;

    public override string Type()
        => _Object.STRING_OBJ;
}

public record Environment
{
    public Dictionary<string, _Object> Store { get; } = new();
    public Environment? Outer { get; set; } = null;

    public static Environment NewEnclosedEnvironment(Environment outer)
    {
        var env = new Environment() { Outer = outer };
        return env;
    }

    public bool Get(string name, out _Object? obj)
    {
        var result = Store.TryGetValue(name, out obj);
        if (!result && Outer is not null)
        {
            result = Outer.Get(name, out obj);
        }
        return result;
    }

    public _Object? Set(string name, _Object val)
    {
        var result = Store.TryAdd(name, val);
        return result ? val : null;
    }
}

public record Function(List<Identifier> Parameters, BlockStatement Body, Environment env) : _Object
{

    public override string Inspect()
    {
        var sb = new StringBuilder();
        List<string> @params = Parameters.Select(p => p.String()).ToList();
        sb.Append("Fn");
        sb.Append("(");
        sb.Append(string.Join(", ", @params));
        sb.Append(") {\n");
        sb.Append(Body.String());
        sb.Append("\n}");
        return sb.ToString();
    }

    public override string Type()
        => _Object.FUNCTION_OBJ;
}

public delegate _Object BuiltinFunction(params _Object[] args);

public record Builtin(BuiltinFunction Fn) : _Object
{
    public override string Inspect()
        => "builtin function";

    public override string Type()
        => _Object.BUILTIN;
}

public record _Array : _Object
{
    public _Object[] Elements { get; set; } = [];

    public override string Inspect()
    {
        var sb = new StringBuilder();
        sb.Append("[");
        sb.Append(Elements.Select(e => e.Inspect()));
        sb.Append("]");
        return sb.ToString();
    }

    public override string Type()
        => _Object.ARRAY;
}
