using Canon.Core.Abstractions;
using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class VarDeclarations : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.VarDeclarations;

    public override void PreVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PreVisit(this);
    }

    public override void PostVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PostVisit(this);
    }

    public static VarDeclarations Create(List<SyntaxNodeBase> children)
    {
        return new VarDeclarations { Children = children };
    }
}
