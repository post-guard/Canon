using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class ValueParameter : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.ValueParameter;

    /// <summary>
    /// 声明的变量列表
    /// </summary>
    public IdentifierList IdentifierList => Children[0].Convert<IdentifierList>();

    /// <summary>
    /// 声明的变量类型
    /// </summary>
    public BasicType BasicType => Children[2].Convert<BasicType>();

    public static ValueParameter Create(List<SyntaxNodeBase> children)
    {
        return new ValueParameter { Children = children };
    }
}
