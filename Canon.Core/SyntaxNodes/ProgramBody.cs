using Canon.Core.CodeGenerators;
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

    public override void GenerateCCode(CCodeBuilder builder)
    {
        //全局常量，变量
        ConstDeclarations.GenerateCCode(builder);
        VarDeclarations.GenerateCCode(builder);
        //子函数声明
        SubprogramDeclarations.GenerateCCode(builder);
        //main函数
        builder.AddString(" int main(){");
        CompoundStatement.GenerateCCode(builder);
        builder.AddString(" return 0;}");
    }
}
