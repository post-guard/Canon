using Canon.Core.CodeGenerators;
using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class ConstValue : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.ConstValue;

    public static ConstValue Create(List<SyntaxNodeBase> children)
    {
        return new ConstValue { Children = children };
    }

    public override void GenerateCCode(CCodeBuilder builder)
    {
        //获取常量值
        var token = Children[0].Convert<TerminatedSyntaxNode>().Token;
        //constValue -> 'letter'
        if (token.TokenType == SemanticTokenType.Character)
        {
            builder.AddString(" '" + token.LiteralValue + "'");
        }
        else
        {
            builder.AddString(" ");
            // constValue -> +num | -num | num
            foreach (var c in Children)
            {
                builder.AddString(c.Convert<TerminatedSyntaxNode>().Token.LiteralValue);
            }
        }
    }
}
