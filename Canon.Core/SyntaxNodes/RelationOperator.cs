using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class RelationOperator : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.RelationOperator;

    public static RelationOperator Create(List<SyntaxNodeBase> children)
    {
        return new RelationOperator { Children = children };
    }
}
