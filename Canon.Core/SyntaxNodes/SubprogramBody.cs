using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class SubprogramBody : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.SubprogramBody;

    /// <summary>
    /// 常量声明部分
    /// </summary>
    public ConstDeclarations ConstDeclarations => Children[0].Convert<ConstDeclarations>();

    /// <summary>
    /// 变量声明部分
    /// </summary>
    public VarDeclarations VarDeclarations => Children[1].Convert<VarDeclarations>();

    /// <summary>
    /// 语句声明部分
    /// </summary>
    public CompoundStatement CompoundStatement => Children[2].Convert<CompoundStatement>();

    public static SubprogramBody Create(List<SyntaxNodeBase> children)
    {
        return new SubprogramBody() { Children = children };
    }
}
