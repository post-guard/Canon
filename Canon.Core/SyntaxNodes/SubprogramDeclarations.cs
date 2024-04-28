using Canon.Core.Abstractions;
using Canon.Core.CodeGenerators;
using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class SubprogramDeclarations : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.SubprogramDeclarations;

    public override void PreVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PreVisit(this);
    }

    public override void PostVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PostVisit(this);
    }

    public static SubprogramDeclarations Create(List<SyntaxNodeBase> children)
    {
        return new SubprogramDeclarations { Children = children };
    }
}
