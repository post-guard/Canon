using Canon.Core.Abstractions;
using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class ParameterList : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.ParameterList;

    public override void PreVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PreVisit(this);
    }

    public override void PostVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PostVisit(this);
    }

    public static ParameterList Create(List<SyntaxNodeBase> children)
    {
        return new ParameterList { Children = children };
    }
}
