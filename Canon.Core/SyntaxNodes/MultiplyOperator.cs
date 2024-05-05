using Canon.Core.Abstractions;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Core.SyntaxNodes;

public class MultiplyOperator : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.MultiplyOperator;

    public SemanticToken OperatorToken => Children[0].Convert<TerminatedSyntaxNode>().Token;

    public override void PreVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PreVisit(this);
    }

    public override void PostVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PostVisit(this);
    }

    public static MultiplyOperator Create(List<SyntaxNodeBase> children)
    {
        return new MultiplyOperator { Children = children };
    }
}
