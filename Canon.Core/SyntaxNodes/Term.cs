using Canon.Core.Abstractions;
using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class FactorGeneratorEventArgs : EventArgs
{
    public required Factor Factor { get; init; }
}

public class MultiplyGeneratorEventArgs : EventArgs
{
    public required Term Left { get; init; }

    public required MultiplyOperator Operator { get; init; }

    public required Factor Right { get; init; }
}

public class Term : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.Term;

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
    public event EventHandler<FactorGeneratorEventArgs>? OnFactorGenerator;

    /// <summary>
    /// 乘法产生式的事件
    /// </summary>
    public event EventHandler<MultiplyGeneratorEventArgs>? OnMultiplyGenerator;

    public static Term Create(List<SyntaxNodeBase> children)
    {
        return new Term { Children = children };
    }

    private void RaiseEvent()
    {
        if (Children.Count == 1)
        {
            OnFactorGenerator?.Invoke(this, new FactorGeneratorEventArgs { Factor = Children[0].Convert<Factor>() });
        }
        else
        {
            OnMultiplyGenerator?.Invoke(this,
                new MultiplyGeneratorEventArgs
                {
                    Left = Children[0].Convert<Term>(),
                    Operator = Children[1].Convert<MultiplyOperator>(),
                    Right = Children[2].Convert<Factor>()
                });
        }

        OnFactorGenerator = null;
        OnMultiplyGenerator = null;
    }
}
