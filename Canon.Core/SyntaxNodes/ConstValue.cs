using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class ConstValue : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.ConstValue;

    public static ConstValue Create(List<SyntaxNodeBase> children)
    {
        return new ConstValue { Children = children };
    }
}
