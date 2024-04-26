using Canon.Core.Abstractions;
using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class FormalParameter : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.FormalParameter;

    public override void PreVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PreVisit(this);
    }

    public override void PostVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PostVisit(this);
    }

    public static FormalParameter Create(List<SyntaxNodeBase> children)
    {
        return new FormalParameter { Children = children };
    }
}
