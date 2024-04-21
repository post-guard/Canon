using Canon.Core.CodeGenerators;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;
using Canon.Core.SemanticParser;

namespace Canon.Core.SyntaxNodes;

public class Variable : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.Variable;

    /// <summary>
    /// 变量的名称
    /// </summary>
    public IdentifierSemanticToken Identifier =>
        (IdentifierSemanticToken)Children[0].Convert<TerminatedSyntaxNode>().Token;

    public static Variable Create(List<SyntaxNodeBase> children)
    {
        return new Variable { Children = children };
    }

    public override void GenerateCCode(CCodeBuilder builder)
    {
        //判断是否为引用变量
        builder.SymbolTable.TryGetSymbol(Identifier.IdentifierName, out var symbol);
        if (symbol is not null && symbol.Reference)
        {
            builder.AddString(" (*" + Identifier.IdentifierName + ")");
        }
        else
        {
            builder.AddString(" " + Identifier.IdentifierName);
        }

        //处理idVarPart（数组下标部分）
        var idVarPart = Children[1].Convert<IdentifierVarPart>();
        if (idVarPart.Exist)
        {
            PascalArrayType pascalArrayType = (PascalArrayType)symbol.SymbolType;
            var positions = idVarPart.Positions;

            foreach (var pos in positions.Reverse())
            {
                builder.AddString("[");
                pos.GenerateCCode(builder);
                //pascal下标减去左边界，从而映射到C语言的下标
                builder.AddString(" - " + System.Convert.ToString(pascalArrayType.Begin) + "]");

                try
                {
                    pascalArrayType = (PascalArrayType)pascalArrayType.ElementType;
                }
                catch (InvalidCastException e)
                {
                    //do nothing
                    //因为最后一层嵌套类型，必然不是PascalArrayType, 而是BasicType
                }
            }
        }
    }
}
