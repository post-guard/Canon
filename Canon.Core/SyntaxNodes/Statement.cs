using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class Statement : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.Statement;

    public static Statement Create(List<SyntaxNodeBase> children)
    {
        return new Statement { Children = children };
    }
}
