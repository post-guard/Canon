using Canon.Core.Abstractions;
using Canon.Core.CodeGenerators;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Core.SyntaxNodes;

public class ProcedureCall : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.ProcedureCall;

    public IdentifierSemanticToken ProcedureId
        => (IdentifierSemanticToken)Children[0].Convert<TerminatedSyntaxNode>().Token;

    public IEnumerable<Expression> Arguments => GetArguments();

    public override void PreVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PreVisit(this);
    }

    public override void PostVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PostVisit(this);
    }

    public static ProcedureCall Create(List<SyntaxNodeBase> children)
    {
        return new ProcedureCall { Children = children };
    }

    private IEnumerable<Expression> GetArguments()
    {
        if (Children.Count == 1)
        {
            yield break;
        }

        foreach (Expression expression in Children[2].Convert<ExpressionList>().Expressions)
        {
            yield return expression;
        }
    }

    public override void GenerateCCode(CCodeBuilder builder)
    {
        builder.AddString(ProcedureId.IdentifierName + "(");

        //用逗号分隔输出的expression
        using (var enumerator = Arguments.GetEnumerator())
        {
            if (enumerator.MoveNext())
            {
                enumerator.Current.GenerateCCode(builder);
            }

            while (enumerator.MoveNext())
            {
                builder.AddString(", ");
                enumerator.Current.GenerateCCode(builder);
            }
        }

        builder.AddString(")");
    }
}
