using Canon.Core.CodeGenerators;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Core.SyntaxNodes;

public class AddOperator : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.AddOperator;

    public static AddOperator Create(List<SyntaxNodeBase> children)
    {
        return new AddOperator { Children = children };
    }

    public override void GenerateCCode(CCodeBuilder builder)
    {
        var operatorType = Children[0].Convert<TerminatedSyntaxNode>().Token.
            Convert<OperatorSemanticToken>().OperatorType;
        if (operatorType == OperatorType.Plus)
        {
            builder.AddString(" +");
        }
        else if (operatorType == OperatorType.Minus)
        {
            builder.AddString(" -");
        }
        else
        {
            builder.AddString(" ||");
        }
    }
}
