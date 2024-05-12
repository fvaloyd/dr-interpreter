namespace Interpreter;

using ObjectType = string;

public abstract record _Object
{
    public const string INTEGER_OBJ = "INTEGER";
    public const string BOOLEAN_OBJ = "BOOLEAN";
    public const string NULL_OBJ = "NULL";
    public const string RETURN_VALUE_OBJ = "RETURN_VALUE";
    public const string ERROR_OBJ = "ERROR";

    public abstract ObjectType Type();
    public abstract string Inspect();
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

