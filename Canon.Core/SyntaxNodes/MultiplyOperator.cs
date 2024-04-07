using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class MultiplyOperator : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.MultiplyOperator;

    public static MultiplyOperator Create(List<SyntaxNodeBase> children)
    {
        return new MultiplyOperator { Children = children };
    }
}
