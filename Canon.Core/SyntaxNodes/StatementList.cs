using Canon.Core.Abstractions;
using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class StatementList : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.StatementList;

    public override void PreVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PreVisit(this);
    }

    public override void PostVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PostVisit(this);
    }

    public static StatementList Create(List<SyntaxNodeBase> children)
    {
        return new StatementList { Children = children};
    }
}
