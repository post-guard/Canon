using Canon.Core.Abstractions;
using Canon.Core.CodeGenerators;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Core.SyntaxNodes;

public class AddOperator : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.AddOperator;

    public SemanticToken OperatorToken => Children[0].Convert<TerminatedSyntaxNode>().Token;

    public static AddOperator Create(List<SyntaxNodeBase> children)
    {
        return new AddOperator { Children = children };
    }

    public override void PreVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PreVisit(this);
    }

    public override void PostVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PostVisit(this);
    }
}
