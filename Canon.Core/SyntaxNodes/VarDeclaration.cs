using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class VarDeclaration : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.VarDeclaration;

    // public bool IsRecursive { get; private init; }

    // /// <summary>
    // /// 声明的变量
    // /// </summary>
    // public (IdentifierList, TypeSyntaxNode) Variable => GetVariable();

    // private (IdentifierList, TypeSyntaxNode) GetVariable()
    // {
    //     if (IsRecursive)
    //     {
    //         return (Children[2].Convert<IdentifierList>(), Children[4].Convert<TypeSyntaxNode>());
    //     }
    //     else
    //     {
    //         return (Children[0].Convert<IdentifierList>(), Children[2].Convert<TypeSyntaxNode>());
    //     }
    // }

    public static VarDeclaration Create(List<SyntaxNodeBase> children)
    {
        /*bool isRecursive;

        if (children.Count == 2)
        {
            isRecursive = false;
        }
        else if (children.Count == 4)
        {
            isRecursive = true;
        }
        else
        {
            throw new InvalidOperationException();
        }*/

        return new VarDeclaration {Children = children};
    }
}
