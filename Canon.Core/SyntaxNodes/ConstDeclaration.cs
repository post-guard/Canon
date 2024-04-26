using Canon.Core.Abstractions;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Core.SyntaxNodes;

public class ConstDeclaration : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.ConstDeclaration;

    /// <summary>
    /// 是否递归的声明下一个ConstDeclaration
    /// </summary>
    public bool IsRecursive { get; private init; }

    /// <summary>
    /// 获得声明的常量
    /// </summary>
    public (IdentifierSemanticToken, ConstValue) ConstValue => GetConstValue();

    public override void PreVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PreVisit(this);
    }

    public override void PostVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PostVisit(this);
    }

    public static ConstDeclaration Create(List<SyntaxNodeBase> children)
    {
        bool isRecursive;
        if (children.Count == 3)
        {
            isRecursive = false;
        }
        else if (children.Count == 5)
        {
            isRecursive = true;
        }
        else
        {
            throw new InvalidOperationException();
        }

        return new ConstDeclaration { Children = children, IsRecursive = isRecursive };
    }

    private static IdentifierSemanticToken ConvertToIdentifierSemanticToken(SyntaxNodeBase node)
    {
        return (IdentifierSemanticToken)node.Convert<TerminatedSyntaxNode>().Token;
    }

    private (IdentifierSemanticToken, ConstValue) GetConstValue()
    {
        if (IsRecursive)
        {
            return (ConvertToIdentifierSemanticToken(Children[2]), Children[4].Convert<ConstValue>());
        }
        else
        {
            return (ConvertToIdentifierSemanticToken(Children[0]), Children[2].Convert<ConstValue>());
        }
    }
}
