using Canon.Core.Abstractions;
using Canon.Core.Enums;
using Canon.Core.SemanticParser;

namespace Canon.Core.SyntaxNodes;

public class OnExpressionListEventArgs : EventArgs
{
    public required ExpressionList ExpressionList { get; init; }
}

public class ExpressionList : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.ExpressionList;

    /// <summary>
    /// 子表达式列表
    /// </summary>
    public List<Expression> Expressions { get; } = [];

    /// <summary>
    /// 是否为传参列表
    /// </summary>
    public bool IsParamList;

    public List<PascalParameterType> ParameterTypes { get; } = [];

    /// <summary>
    /// 是否为数组下标索引
    /// </summary>
    public bool IsIndex { get; set; }

    /// <summary>
    /// 数组左边界列表
    /// </summary>
    public List<int> LeftBounds = new();

    /// <summary>
    /// 当前ExpressionList中的Expression定义
    /// </summary>
    public required Expression Expression { get; init; }

    public override void PreVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PreVisit(this);
        RaiseEvent();
    }

    public override void PostVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PostVisit(this);
        RaiseEvent();
    }

    /// <summary>
    /// 使用ExpressionList产生式的时间
    /// </summary>
    public event EventHandler<OnExpressionListEventArgs>? OnExpressionList;

    public static ExpressionList Create(List<SyntaxNodeBase> children)
    {
        ExpressionList result;

        if (children.Count == 1)
        {
            result = new ExpressionList { Expression = children[0].Convert<Expression>(), Children = children };
            result.Expressions.Add(children[0].Convert<Expression>());
        }
        else
        {
            result = new ExpressionList { Expression = children[2].Convert<Expression>(), Children = children };
            foreach (Expression expression in children[0].Convert<ExpressionList>().Expressions)
            {
                result.Expressions.Add(expression);
            }

            result.Expressions.Add(children[2].Convert<Expression>());
        }

        return result;
    }

    private void RaiseEvent()
    {
        if (Children.Count == 3)
        {
            OnExpressionList?.Invoke(this,
                new OnExpressionListEventArgs { ExpressionList = Children[0].Convert<ExpressionList>() });
        }

        OnExpressionList = null;
    }
}
