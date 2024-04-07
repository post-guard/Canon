using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class AddOperator : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.AddOperator;

    public static AddOperator Create(List<SyntaxNodeBase> children)
    {
        return new AddOperator { Children = children };
    }
}
