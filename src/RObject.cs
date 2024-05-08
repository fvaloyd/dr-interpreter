namespace Interpreter;

using ObjectType = string;

public abstract record _Object
{
    public const string INTEGER_OBJ = "INTEGER";
    public const string BOOLEAN_OBJ = "BOOLEAN";
    public const string NULL_OBJ = "NULL";

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
