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

public record LetStatement : Statement
{
    Token Token { get; set; } = null!;
    Expression Value { get; set; } = null!;
    Identifier Identifier { get; set; } = null!;

    public string TokenLiteral()
    {
        return Token.Literal;
    }
}

public record Identifier(Token Token, string Value) : Expression
{
    public string TokenLiteral()
    {
        return Token.Literal;
    }
}
