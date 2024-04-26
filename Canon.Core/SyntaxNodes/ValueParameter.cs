using Canon.Core.Abstractions;
using Canon.Core.CodeGenerators;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Core.SyntaxNodes;

public class ValueParameter : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.ValueParameter;

    /// <summary>
    /// 是否为参数中的引用参数
    /// </summary>
    public bool IsReference { get; set; }

    public IdentifierSemanticToken Token =>
        Children[0].Convert<TerminatedSyntaxNode>().Token.Convert<IdentifierSemanticToken>();

    public IdentifierList IdentifierList => Children[1].Convert<IdentifierList>();

    public override void PreVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PreVisit(this);
    }

    public override void PostVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PostVisit(this);
    }

    public static ValueParameter Create(List<SyntaxNodeBase> children)
    {
        return new ValueParameter { Children = children };
    }
}
