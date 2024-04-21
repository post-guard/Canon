using Canon.Core.CodeGenerators;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Core.SyntaxNodes;

public class IdentifierList : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.IdentifierList;

    /// <summary>
    /// 是否含有递归定义
    /// </summary>
    public bool IsRecursive { get; private init; }

    /// <summary>
    /// 声明的标识符列表
    /// </summary>
    public IEnumerable<IdentifierSemanticToken> Identifiers => GetIdentifiers();

    public static IdentifierList Create(List<SyntaxNodeBase> children)
    {
        bool isRecursive;

        if (children.Count == 1)
        {
            isRecursive = false;
        }
        else if (children.Count == 3)
        {
            isRecursive = true;
        }
        else
        {
            throw new InvalidOperationException();
        }

        return new IdentifierList { IsRecursive = isRecursive, Children = children };
    }

    private IEnumerable<IdentifierSemanticToken> GetIdentifiers()
    {
        IdentifierList identifier = this;

        while (true)
        {
            if (identifier.IsRecursive)
            {
                yield return (IdentifierSemanticToken)identifier.Children[2].Convert<TerminatedSyntaxNode>().Token;
                identifier = identifier.Children[0].Convert<IdentifierList>();
            }
            else
            {
                yield return (IdentifierSemanticToken)identifier.Children[0].Convert<TerminatedSyntaxNode>().Token;
                break;
            }
        }
    }

    public override void GenerateCCode(CCodeBuilder builder)
    {
        //用逗号分隔输出的expression
        using var enumerator = Identifiers.Reverse().GetEnumerator();

        if (enumerator.MoveNext())
        {
            builder.AddString(" " + enumerator.Current.IdentifierName);
        }

        while (enumerator.MoveNext())
        {
            builder.AddString(", " + enumerator.Current.IdentifierName);
        }
    }
}
