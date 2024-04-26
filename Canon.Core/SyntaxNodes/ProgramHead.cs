using Canon.Core.Abstractions;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Core.SyntaxNodes;

public class ProgramHead : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.ProgramHead;

    /// <summary>
    /// 程序名称
    /// </summary>
    public IdentifierSemanticToken ProgramName
        => (IdentifierSemanticToken)Children[1].Convert<TerminatedSyntaxNode>().Token;

    public override void PreVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PreVisit(this);
    }

    public override void PostVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PostVisit(this);
    }

    public static ProgramHead Create(List<SyntaxNodeBase> children)
    {
        return new ProgramHead { Children = children };
    }
}
