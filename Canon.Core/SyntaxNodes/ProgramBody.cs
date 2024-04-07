using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class ProgramBody : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.ProgramBody;

    /// <summary>
    /// 常量声明
    /// </summary>
    public ConstDeclarations ConstDeclarations => Children[0].Convert<ConstDeclarations>();

    /// <summary>
    /// 变量声明
    /// </summary>
    public VarDeclarations VarDeclarations => Children[1].Convert<VarDeclarations>();

    /// <summary>
    /// 子程序声明
    /// </summary>
    public SubprogramDeclarations SubprogramDeclarations => Children[2].Convert<SubprogramDeclarations>();

    /// <summary>
    /// 语句声明
    /// </summary>
    public CompoundStatement CompoundStatement => Children[3].Convert<CompoundStatement>();

    public static ProgramBody Create(List<SyntaxNodeBase> children)
    {
        return new ProgramBody { Children = children };
    }
}
