using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Core.SyntaxNodes;

public class ProcedureCall : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.ProcedureCall;

    public IdentifierSemanticToken ProcedureId
        => (IdentifierSemanticToken)Children[0].Convert<TerminatedSyntaxNode>().Token;

    public IEnumerable<Expression> Arguments => GetArguments();

    public static ProcedureCall Create(List<SyntaxNodeBase> children)
    {
        return new ProcedureCall { Children = children };
    }

    private IEnumerable<Expression> GetArguments()
    {
        if (Children.Count == 1)
        {
            yield break;
        }

        foreach (Expression expression in Children[2].Convert<ExpressionList>().Expressions)
        {
            yield return expression;
        }
    }
}
