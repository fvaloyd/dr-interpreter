using System.Text;

namespace Interpreter;

public interface Node
{
    string TokenLiteral();
    string String();
}

public interface Statement : Node { }

public interface Expression : Node { }

public record Program : Node
{
    public List<Statement> Statements { get; set; } = new();

    public string TokenLiteral()
    {
        if (Statements.Count > 0)
        {
            return Statements[0].TokenLiteral();
        }
        return "";
    }

    public string String()
    {
        StringBuilder sb = new();
        foreach (var stmt in Statements)
        {
            sb.Append(stmt.String());
        }
        return sb.ToString();
    }
}

public record LetStatement : Statement
{
    public Token Token { get; set; } = null!;
    public Expression Value { get; set; } = null!;
    public Identifier Name { get; set; } = null!;

    public string String()
    {
        StringBuilder sb = new();
        sb.Append(TokenLiteral()).Append(" ");
        sb.Append(Name.String());
        sb.Append(" = ");

        if (Value is not null)
        {
            sb.Append(Value.String());
        }
        sb.Append(";");
        return sb.ToString();
    }

    public string TokenLiteral()
    {
        return Token.Literal;
    }
}

public record Boolean(Token Token, bool Value) : Expression
{
    public string String()
        => Token.Literal;

    public string TokenLiteral()
        => Token.Literal;
}

public record Identifier(Token Token, string Value) : Expression
{
    public string String()
        => Value;

    public string TokenLiteral()
    {
        return Token.Literal;
    }
}

public record IntegerLiteral(Token Token, Int64 Value) : Expression
{
    public string String()
        => Value.ToString();

    public string TokenLiteral()
        => Token.Literal;
}

public record ReturnStatement : Statement
{
    public Token Token { get; set; } = null!;
    public Expression ReturnValue { get; set; } = null!;

    public string String()
    {
        StringBuilder sb = new();
        sb.Append(TokenLiteral()).Append(" ");
        if (ReturnValue is not null)
        {
            sb.Append(ReturnValue.String());
        }
        sb.Append(";");
        return sb.ToString();
    }

    public string TokenLiteral()
        => Token.Literal;
}

public record ExpressionStatement : Statement
{
    public Token Token { get; set; } = null!;
    public Expression Expression { get; set; } = null!;

    public string String()
    {
        if (Expression is not null)
        {
            return Expression.String();
        }
        return string.Empty;
    }

    public string TokenLiteral()
        => Token.Literal;
}

public record PrefixExpression : Expression
{
    public Token Token { get; set; } = null!;
    public string Operator { get; set; } = string.Empty;
    public Expression Right { get; set; } = null!;

    public string String()
    {
        var sb = new StringBuilder();
        sb.Append("(");
        sb.Append(Operator);
        sb.Append(Right.String());
        sb.Append(")");
        return sb.ToString();
    }

    public string TokenLiteral()
        => Token.Literal;
}

public record InfixExpression : Expression
{
    public Token Token { get; set; } = null!;
    public string Operator { get; set; } = string.Empty;
    public Expression Right { get; set; } = null!;
    public Expression Left { get; set; } = null!;

    public string String()
    {
        var sb = new StringBuilder();
        sb.Append("(");
        sb.Append(Left.String());
        sb.Append(" " + Operator + " ");
        sb.Append(Right.String());
        sb.Append(")");
        return sb.ToString();
    }

    public string TokenLiteral()
        => Token.Literal;
}

public record IfExpression : Expression
{
    public Token Token { get; set; } = null!;
    public Expression Codition { get; set; } = null!;
    public BlockStatement Consequence { get; set; } = null!;
    public BlockStatement? Alternative { get; set; } = null;

    public string String()
    {
        var sb = new StringBuilder();
        sb.Append("if");
        sb.Append(Codition.String());
        sb.Append(" ");
        sb.Append(Consequence.String());
        if (Alternative is not null)
        {
            sb.Append("else");
            sb.Append(Alternative.String());
        }
        return sb.ToString();
    }

    public string TokenLiteral()
        => Token.Literal;
}

public record BlockStatement : Statement
{
    public Token Token { get; set; } = null!;
    public List<Statement> Statements { get; set; } = [];

    public string String()
    {
        var sb = new StringBuilder();
        foreach (var stmt in Statements)
        {
            sb.Append(stmt.String());
        }
        return sb.ToString();
    }

    public string TokenLiteral()
        => Token.Literal;
}

public record FunctionLiteral : Expression
{
    public Token Token { get; set; } = null!;
    public List<Identifier> Parameters { get; set; } = [];
    public BlockStatement Body { get; set; } = null!;

    public string String()
    {
        var sb = new StringBuilder();
        var @params = Parameters.Select(x => x.String()).ToList();
        sb.Append(TokenLiteral());
        sb.Append("(");
        sb.Append(string.Join(", ", @params));
        sb.Append(") ");
        sb.Append(Body.String());

        return sb.ToString();
    }

    public string TokenLiteral()
        => Token.Literal;
}

public record CallExpression : Expression
{
    public Token Token { get; set; } = null!;
    public Expression Function { get; set; } = null!;
    public List<Expression> Arguments { get; set; } = [];

    public string String()
    {
        var sb = new StringBuilder();
        var args = Arguments.Select(x => x.String()).ToList();
        sb.Append(Function.String());
        sb.Append("(");
        sb.Append(string.Join(", ", args));
        sb.Append(")");

        return sb.ToString();
    }

    public string TokenLiteral()
        => Token.Literal;
}

public record StringLiteral(Token Token, string Value) : Expression
{
    public string String()
        => Token.Literal;

    public string TokenLiteral()
        => Token.Literal;
}
