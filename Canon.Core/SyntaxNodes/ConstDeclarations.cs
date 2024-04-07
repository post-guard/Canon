using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Core.SyntaxNodes;

public class ConstDeclarations : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.ConstDeclarations;

    /// <summary>
    /// 声明的常量列表
    /// </summary>
    public IEnumerable<(IdentifierSemanticToken, ConstValue)> ConstValues => GetConstValues();

    public static ConstDeclarations Create(List<SyntaxNodeBase> children)
    {
        return new ConstDeclarations { Children = children };
    }

    private IEnumerable<(IdentifierSemanticToken, ConstValue)> GetConstValues()
    {
        if (Children.Count == 0)
        {
            yield break;
        }

        ConstDeclaration declaration = Children[1].Convert<ConstDeclaration>();

        while (true)
        {
            yield return declaration.ConstValue;

            if (declaration.IsRecursive)
            {
                declaration = declaration.Children[0].Convert<ConstDeclaration>();
            }
            else
            {
                break;
            }
        }
    }
}
