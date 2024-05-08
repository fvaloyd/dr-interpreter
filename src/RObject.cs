namespace Interpreter;

using ObjectType = string;

public interface RObject
{
    ObjectType Type();
    string Inspect();
}
