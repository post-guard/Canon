using Canon.Core.Abstractions;
using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class ExpressionList : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.ExpressionList;

    /// <summary>
    /// 子表达式列表
    /// </summary>
    public List<Expression> Expressions { get; } = [];

    public override void PreVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PreVisit(this);
    }

    public override void PostVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PostVisit(this);
    }

    public static ExpressionList Create(List<SyntaxNodeBase> children)
    {
        ExpressionList result = new() { Children = children };

        if (children.Count == 1)
        {
            result.Expressions.Add(children[0].Convert<Expression>());
        }
        else if (children.Count == 3)
        {
            foreach (Expression expression in children[0].Convert<ExpressionList>().Expressions)
            {
                result.Expressions.Add(expression);
            }

            result.Expressions.Add(children[2].Convert<Expression>());
        }

        return result;
    }
}
