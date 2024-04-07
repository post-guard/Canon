using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class Factor : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.Factor;

    public static Factor Create(List<SyntaxNodeBase> children)
    {
        return new Factor { Children = children };
    }
}
