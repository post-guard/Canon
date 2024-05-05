using Canon.Core.Abstractions;
using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class Subprogram : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.Subprogram;

    public override void PreVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PreVisit(this);
    }

    public override void PostVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PostVisit(this);
    }

    public static Subprogram Create(List<SyntaxNodeBase> children)
    {
        return new Subprogram { Children = children };
    }
}
