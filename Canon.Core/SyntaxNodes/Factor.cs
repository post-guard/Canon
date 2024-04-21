using Canon.Core.CodeGenerators;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Core.SyntaxNodes;

public class Factor : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.Factor;

    public static Factor Create(List<SyntaxNodeBase> children)
    {
        return new Factor { Children = children };
    }

    public override void GenerateCCode(CCodeBuilder builder)
    {
        if (Children.Count == 1)
        {
            //factor -> num
            if (Children[0].IsTerminated)
            {
                var token = Children[0].Convert<TerminatedSyntaxNode>().Token;
                if (token.TokenType == SemanticTokenType.Number)
                {
                    builder.AddString(" " + token.LiteralValue);
                }
            }
            // factor -> variable
            else
            {
                Children[0].GenerateCCode(builder);
            }
        }
        //factor -> ( expression )
        else if (Children.Count == 3)
        {
            builder.AddString(" (");
            Children[1].GenerateCCode(builder);
            builder.AddString(")");
        }
        //factor -> id ( expression )
        else if (Children.Count == 4)
        {
            builder.AddString(" " + Children[0].Convert<TerminatedSyntaxNode>().Token.
                Convert<IdentifierSemanticToken>().IdentifierName);
            builder.AddString("(");
            Children[2].GenerateCCode(builder);
            builder.AddString(")");
        }
        else
        {   //factor -> not factor
            builder.AddString(" (");
            if (Children[0].Convert<TerminatedSyntaxNode>().Token.TokenType == SemanticTokenType.Keyword)
            {
                builder.AddString("!");
            }
            else
            {
                builder.AddString("-");
            }

            Children[1].GenerateCCode(builder);
            builder.AddString(")");
        }
    }
}
