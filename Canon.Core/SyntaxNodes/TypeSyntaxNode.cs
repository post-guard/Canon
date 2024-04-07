using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class TypeSyntaxNode : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.Type;

    public static TypeSyntaxNode Create(List<SyntaxNodeBase> children)
    {
        return new TypeSyntaxNode { Children = children };
    }
}
