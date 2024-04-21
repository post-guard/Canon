using Canon.Core.CodeGenerators;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Core.SyntaxNodes;

public class MultiplyOperator : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.MultiplyOperator;

    public static MultiplyOperator Create(List<SyntaxNodeBase> children)
    {
        return new MultiplyOperator { Children = children };
    }

    public override void GenerateCCode(CCodeBuilder builder)
    {
        var token = Children[0].Convert<TerminatedSyntaxNode>().Token;
        if (token.TokenType == SemanticTokenType.Operator)
        {
            var operatorType = token.Convert<OperatorSemanticToken>().OperatorType;
            if (operatorType == OperatorType.Multiply)
            {
                builder.AddString(" *");
            }
            else if (operatorType == OperatorType.Divide)
            {
                //实数除法，需要将操作数强转为float
                builder.AddString(" /(double)");
            }
        }
        else
        {
            var keywordType = token.Convert<KeywordSemanticToken>().KeywordType;
            if (keywordType == KeywordType.And)
            {
                builder.AddString(" &&");
            }
            else if (keywordType == KeywordType.Mod)
            {
                builder.AddString(" %");
            }
            else
            {
                builder.AddString(" /");
            }
        }

    }
}
