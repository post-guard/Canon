using Canon.Core.Abstractions;
using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class ConstDeclarations : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.ConstDeclarations;

    public override void PreVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PreVisit(this);
    }

    public override void PostVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PostVisit(this);
    }

    public static ConstDeclarations Create(List<SyntaxNodeBase> children)
    {
        return new ConstDeclarations { Children = children };
    }
}
