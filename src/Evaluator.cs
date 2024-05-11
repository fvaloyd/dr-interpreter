namespace Interpreter;

public class Evaluator
{
    public static _Object? Eval(Node node)
        => node switch
        {
            Program pr => evalStatements(pr.Statements),
            ExpressionStatement es => Eval(es.Expression),
            IntegerLiteral il => new Integer(il.Value),
            Boolean b => new _Boolean(b.Value),
            _ => null
        };

    public static _Object? evalStatements(List<Statement> stmts)
    {
        _Object? result = null;
        foreach (var stmt in stmts)
        {
            result = Eval(stmt);
        }
        return result;
    }
}
