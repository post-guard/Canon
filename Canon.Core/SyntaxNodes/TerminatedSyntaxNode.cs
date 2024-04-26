using Canon.Core.Abstractions;
using Canon.Core.LexicalParser;

namespace Canon.Core.SyntaxNodes;

public class TerminatedSyntaxNode : SyntaxNodeBase
{
    public override bool IsTerminated => true;

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
