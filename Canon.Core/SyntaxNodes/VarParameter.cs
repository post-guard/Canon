using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class VarParameter : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.VarParameter;

    public ValueParameter ValueParameter => Children[1].Convert<ValueParameter>();

    public static VarParameter Create(List<SyntaxNodeBase> children)
    {
        return new VarParameter { Children = children };
    }
}
