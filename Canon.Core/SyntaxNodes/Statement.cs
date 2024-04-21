using Canon.Core.CodeGenerators;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Core.SyntaxNodes;

public class Statement : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.Statement;

    public static Statement Create(List<SyntaxNodeBase> children)
    {
        return new Statement { Children = children };
    }

    public override void GenerateCCode(CCodeBuilder builder)
    {
        if (Children.Count == 0)
        {
            return;
        }
        // statement -> procedureCall | compoundStatement
        if (Children.Count == 1)
        {
            Children[0].GenerateCCode(builder);
        }
        //statement -> variable assign expression
        else if (Children.Count == 3)
        {
            Children[0].GenerateCCode(builder);
            builder.AddString(" =");
            Children[2].GenerateCCode(builder);
        }
        //if expression then statement else_part
        else if (Children.Count == 5)
        {
            builder.AddString(" if(");
            Children[1].GenerateCCode(builder);
            builder.AddString("){");
            Children[3].GenerateCCode(builder);
            builder.AddString("; }");
            Children[4].GenerateCCode(builder);
        }
        //for id assign expression to expression do statement
        else
        {
            string idName = Children[1].Convert<TerminatedSyntaxNode>().Token.Convert<IdentifierSemanticToken>()
                .IdentifierName;
            builder.AddString(" for(" + idName + " =");
            Children[3].GenerateCCode(builder);
            builder.AddString("; " + idName + " <=");
            Children[5].GenerateCCode(builder);
            builder.AddString("; " + idName + "++){");
            Children[7].GenerateCCode(builder);
            builder.AddString("; }");
        }
    }
}
