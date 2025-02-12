﻿using Canon.Core.Abstractions;
using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class TermGeneratorEventArgs : EventArgs
{
    public required Term Term { get; init; }
}

public class AddGeneratorEventArgs : EventArgs
{
    public required SimpleExpression Left { get; init; }

    public required AddOperator Operator { get; init; }

    public required Term Right { get; init; }
}

public class SimpleExpression : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.SimpleExpression;

    /// <summary>
    /// 是否为条件判断语句
    /// </summary>
    public bool IsCondition { get; set; }

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
    /// 直接赋值产生式的事件
    /// </summary>
    public event EventHandler<TermGeneratorEventArgs>? OnTermGenerator;

    /// <summary>
    /// 加法产生式的事件
    /// </summary>
    public event EventHandler<AddGeneratorEventArgs>? OnAddGenerator;

    public static SimpleExpression Create(List<SyntaxNodeBase> children)
    {
        return new SimpleExpression { Children = children };
    }

    private void RaiseEvent()
    {
        if (Children.Count == 1)
        {
            OnTermGenerator?.Invoke(this, new TermGeneratorEventArgs { Term = Children[0].Convert<Term>() });
        }
        else
        {
            OnAddGenerator?.Invoke(this,
                new AddGeneratorEventArgs
                {
                    Left = Children[0].Convert<SimpleExpression>(),
                    Operator = Children[1].Convert<AddOperator>(),
                    Right = Children[2].Convert<Term>()
                });
        }

        OnTermGenerator = null;
        OnAddGenerator = null;
    }
}
