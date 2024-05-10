using Canon.Core.Abstractions;
using Canon.Core.LexicalParser;

namespace Canon.Core.SyntaxNodes;

public class TerminatedSyntaxNode : SyntaxNodeBase
{
    public override bool IsTerminated => true;

    /// <summary>
    /// 是否为For循环定义中的DO节点
    /// </summary>
    public bool IsForNode { get; set; }

    /// <summary>
    /// 是否为While循环定义中的DO节点
    /// </summary>
    public bool IsWhileNode { get; set; }

    public override void PreVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PreVisit(this);
    }

    public override void PostVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PostVisit(this);
    }

    public required SemanticToken Token { get; init; }

    public override string ToString()
    {
        return Token.LiteralValue;
    }
}
