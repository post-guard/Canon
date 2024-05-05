using Canon.Core.Abstractions;
using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class SimpleExpressionGeneratorEventArgs : EventArgs
{
    public required SimpleExpression SimpleExpression { get; init; }
}

public class RelationGeneratorEventArgs : EventArgs
{
    public required SimpleExpression Left { get; init; }

    public required RelationOperator Operator { get; init; }

    public required SimpleExpression Right { get; init; }
}

public class Expression : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.Expression;

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
    /// 是否为FOR语句中的起始语句
    /// </summary>
    public bool IsForConditionBegin { get; set; }

    /// <summary>
    /// 是否为FOR语句中的结束语句
    /// </summary>
    public bool IsForConditionEnd { get; set; }

    /// <summary>
    /// 是否为IF语句中的条件语句
    /// </summary>
    public bool IsIfCondition { get; set; }

    /// <summary>
    /// 直接赋值产生式的事件
    /// </summary>
    public event EventHandler<SimpleExpressionGeneratorEventArgs>? OnSimpleExpressionGenerator;

    /// <summary>
    /// 关系产生式的事件
    /// </summary>
    public event EventHandler<RelationGeneratorEventArgs>? OnRelationGenerator;

    public static Expression Create(List<SyntaxNodeBase> children)
    {
        return new Expression { Children = children };
    }

    private void RaiseEvent()
    {
        if (Children.Count == 1)
        {
            OnSimpleExpressionGenerator?.Invoke(this, new SimpleExpressionGeneratorEventArgs
            {
                SimpleExpression = Children[0].Convert<SimpleExpression>()
            });
        }
        else
        {
            OnRelationGenerator?.Invoke(this, new RelationGeneratorEventArgs
            {
                Left = Children[0].Convert<SimpleExpression>(),
                Operator = Children[1].Convert<RelationOperator>(),
                Right = Children[2].Convert<SimpleExpression>()
            });
        }

        OnSimpleExpressionGenerator = null;
        OnRelationGenerator = null;
    }
}
