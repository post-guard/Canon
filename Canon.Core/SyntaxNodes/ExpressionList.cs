using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class ExpressionList : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.ExpressionList;

    public bool IsRecursive { get; private init; }

    /// <summary>
    /// 声明的表达式列表
    /// </summary>
    public IEnumerable<Expression> Expressions => GetExpressions();

    public static ExpressionList Create(List<SyntaxNodeBase> children)
    {
        bool isRecursive;

        if (children.Count == 1)
        {
            isRecursive = false;
        }
        else if (children.Count == 3)
        {
            isRecursive = true;
        }
        else
        {
            throw new InvalidOperationException();
        }

        return new ExpressionList { Children = children, IsRecursive = isRecursive };
    }

    private IEnumerable<Expression> GetExpressions()
    {
        ExpressionList list = this;

        while (true)
        {
            if (list.IsRecursive)
            {
                yield return list.Children[2].Convert<Expression>();
                list = list.Children[0].Convert<ExpressionList>();
            }
            else
            {
                yield return list.Children[0].Convert<Expression>();
                break;
            }
        }
    }
}
