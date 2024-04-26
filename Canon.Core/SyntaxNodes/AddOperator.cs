using Canon.Core.Abstractions;
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
        var token = Children[0].Convert<TerminatedSyntaxNode>().Token;
        if (token.TokenType == SemanticTokenType.Operator)
        {
            var operatorType = token.Convert<OperatorSemanticToken>().OperatorType;
            if (operatorType == OperatorType.Plus)
            {
                builder.AddString(" +");
            }
            else if (operatorType == OperatorType.Minus)
            {
                builder.AddString(" -");
            }
        }
        else
        {
            builder.AddString(" ||");
        }
    }

    public override void PreVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PreVisit(this);
    }

    public override void PostVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PostVisit(this);
    }
}
