using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class ElsePart : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.ElsePart;

    public static ElsePart Create(List<SyntaxNodeBase> children)
    {
        return new ElsePart { Children = children };
    }
}
