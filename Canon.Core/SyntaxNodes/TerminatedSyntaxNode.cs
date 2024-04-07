using Canon.Core.LexicalParser;

namespace Canon.Core.SyntaxNodes;

public class TerminatedSyntaxNode : SyntaxNodeBase
{
    public override bool IsTerminated => true;

    public required SemanticToken Token { get; init; }
}
