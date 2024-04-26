using Canon.Core.Abstractions;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Core.SyntaxNodes;

public class VarDeclaration : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.VarDeclaration;

    public override void PreVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PreVisit(this);
    }

    public override void PostVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PostVisit(this);
    }

    public required IdentifierSemanticToken Token { get; init; }

    public required IdentifierList IdentifierList { get; init; }

    public static VarDeclaration Create(List<SyntaxNodeBase> children)
    {
        if (children.Count == 2)
        {
            return new VarDeclaration
            {
                Children = children,
                Token = children[0].Convert<TerminatedSyntaxNode>().Token.Convert<IdentifierSemanticToken>(),
                IdentifierList = children[1].Convert<IdentifierList>()
            };
        }
        else
        {
            return new VarDeclaration
            {
                Children = children,
                Token = children[2].Convert<TerminatedSyntaxNode>().Token.Convert<IdentifierSemanticToken>(),
                IdentifierList = children[3].Convert<IdentifierList>()
            };
        }
    }
}
