using Canon.Core.Abstractions;
using Canon.Core.CodeGenerators;
using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class VarParameter : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.VarParameter;

    public ValueParameter ValueParameter => Children[1].Convert<ValueParameter>();

    public override void PreVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PreVisit(this);
    }

    public override void PostVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PostVisit(this);
    }

    public static VarParameter Create(List<SyntaxNodeBase> children)
    {
        return new VarParameter { Children = children };
    }
}
