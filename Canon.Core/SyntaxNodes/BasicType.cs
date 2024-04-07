using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class BasicType : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.BasicType;

    public static BasicType Create(List<SyntaxNodeBase> children)
    {
        return new BasicType { Children = children };
    }
}
