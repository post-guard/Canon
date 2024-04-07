using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class Expression : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.Expression;

    public static Expression Create(List<SyntaxNodeBase> children)
    {
        return new Expression { Children = children };
    }
}
