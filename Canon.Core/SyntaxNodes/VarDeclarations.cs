using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class VarDeclarations : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.VarDeclarations;

    /// <summary>
    /// 声明的变量列表
    /// </summary>
    public IEnumerable<(IdentifierList, TypeSyntaxNode)> Variables => EnumerateVariables();

    public static VarDeclarations Create(List<SyntaxNodeBase> children)
    {
        return new VarDeclarations { Children = children };
    }

    private IEnumerable<(IdentifierList, TypeSyntaxNode)> EnumerateVariables()
    {
        if (Children.Count == 0)
        {
            yield break;
        }

        VarDeclaration declaration = Children[1].Convert<VarDeclaration>();

        while (true)
        {
            yield return declaration.Variable;

            if (declaration.IsRecursive)
            {
                declaration = declaration.Children[0].Convert<VarDeclaration>();
            }
            else
            {
                break;
            }
        }
    }
}
