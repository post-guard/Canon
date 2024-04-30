using Canon.Core.Abstractions;
using Canon.Core.CodeGenerators;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Core.SyntaxNodes;

public class RelationOperator : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.RelationOperator;

    public SemanticToken OperatorToken => Children[0].Convert<TerminatedSyntaxNode>().Token;

    public override void PreVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PreVisit(this);
    }

    public override void PostVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PostVisit(this);
    }

    public static RelationOperator Create(List<SyntaxNodeBase> children)
    {
        return new RelationOperator { Children = children };
    }
}
