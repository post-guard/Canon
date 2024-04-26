using Canon.Core.Abstractions;
using Canon.Core.CodeGenerators;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Core.SyntaxNodes;

public class RelationOperator : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.RelationOperator;

    public override void PreVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PreVisit(this);
    }

    public override void PostVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PostVisit(this);
    }

    public static RelationOperator Create(List<SyntaxNodeBase> children)
    {
        return new RelationOperator { Children = children };
    }

    public override void GenerateCCode(CCodeBuilder builder)
    {
        var operatorType = Children[0].Convert<TerminatedSyntaxNode>().Token.Convert<OperatorSemanticToken>()
            .OperatorType;
        switch (operatorType)
        {
            case OperatorType.Equal:
                builder.AddString(" ==");
                break;
            case OperatorType.Greater:
                builder.AddString(" >");
                break;
            case OperatorType.Less:
                builder.AddString(" <");
                break;
            case OperatorType.GreaterEqual:
                builder.AddString(" >=");
                break;
            case OperatorType.LessEqual:
                builder.AddString(" <=");
                break;
            case OperatorType.NotEqual:
                builder.AddString(" !=");
                break;
        }
    }
}
