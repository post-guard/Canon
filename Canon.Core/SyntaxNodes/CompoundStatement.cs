using Canon.Core.Abstractions;
using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class CompoundStatement : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.CompoundStatement;

    public override void PreVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PreVisit(this);
    }

    public override void PostVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PostVisit(this);
    }

    public static CompoundStatement Create(List<SyntaxNodeBase> children)
    {
        return new CompoundStatement { Children = children };
    }
}
