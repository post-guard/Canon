using Canon.Core.CodeGenerators;
using Canon.Core.Enums;
using Canon.Core.SemanticParser;

namespace Canon.Core.SyntaxNodes;

public class VarDeclarations : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.VarDeclarations;

    /// <summary>
    /// 声明的变量列表
    /// </summary>
    // public IEnumerable<(IdentifierList, TypeSyntaxNode)> Variables => EnumerateVariables();

    public static VarDeclarations Create(List<SyntaxNodeBase> children)
    {
        return new VarDeclarations { Children = children };
    }

    // private IEnumerable<(IdentifierList, TypeSyntaxNode)> EnumerateVariables()
    // {
    //     if (Children.Count == 0)
    //     {
    //         yield break;
    //     }
    //
    //     VarDeclaration declaration = Children[1].Convert<VarDeclaration>();
    //
    //     while (true)
    //     {
    //         yield return declaration.Variable;
    //
    //         if (declaration.IsRecursive)
    //         {
    //             declaration = declaration.Children[0].Convert<VarDeclaration>();
    //         }
    //         else
    //         {
    //             break;
    //         }
    //     }
    // }

    // public override void GenerateCCode(CCodeBuilder builder)
    // {
    //     foreach (var pair in Variables.Reverse())
    //     {
    //         //BasicType定义
    //         if (pair.Item2.Children.Count == 1)
    //         {
    //             //输出类型
    //             pair.Item2.GenerateCCode(builder);
    //             //输出idList
    //             pair.Item1.GenerateCCode(builder);
    //             builder.AddString(";");
    //         }
    //         //array定义
    //         else
    //         {
    //             //构造出C语言形式的数组下标定义
    //             string arrayPeriod = "";
    //             var ranges = pair.Item2.Children[2]
    //                 .Convert<Period>().Ranges;
    //             PascalType pascalType = pair.Item2.Children[5].Convert<BasicType>().TryGetPascalType();
    //
    //             foreach (var range in ranges)
    //             {
    //                 int low = int.Parse(range.Item1.LiteralValue);
    //                 int high = int.Parse(range.Item2.LiteralValue);
    //                 arrayPeriod = "[" + System.Convert.ToString(high-low+1) + "]" + arrayPeriod;
    //                 pascalType = new PascalArrayType(pascalType, low, high); //嵌套地构造出多维数组
    //             }
    //
    //             //依次定义每一个符号
    //             foreach (var id in pair.Item1.Identifiers.Reverse())
    //             {
    //                 pair.Item2.Children[5].GenerateCCode(builder);
    //                 builder.AddString(" " + id.IdentifierName + arrayPeriod + ";");
    //                 //写入符号表
    //                 builder.SymbolTable.TryAddSymbol(new Symbol()
    //                 {
    //                     SymbolName = id.IdentifierName, SymbolType = pascalType, Reference = false
    //                 });
    //             }
    //         }
    //     }
    // }
}
