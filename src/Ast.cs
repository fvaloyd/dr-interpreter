namespace Interpreter;

public interface Node
{
    string TokenLiteral();
}

public interface Statement : Node { }

public interface Expression : Node { }

public record Program : Node
{
    public List<Statement> Statements { get; private set; } = new();

    public string TokenLiteral()
    {
        if (Statements.Count > 0)
        {
            return Statements[0].TokenLiteral();
        }
        return "";
    }
}
