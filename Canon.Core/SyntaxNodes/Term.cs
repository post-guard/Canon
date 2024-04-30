using Canon.Core.Abstractions;
using Canon.Core.CodeGenerators;
using Canon.Core.Enums;
using Canon.Core.SemanticParser;

namespace Canon.Core.SyntaxNodes;

public class OnFactorGeneratorEventArgs : EventArgs
{
    public required Factor Factor { get; init; }
}

public class OnMultiplyGeneratorEventArgs : EventArgs
{
    public required Term Left { get; init; }

    public required MultiplyOperator Operator { get; init; }

    public required Factor Right { get; init; }
}

public class Term : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.Term;

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
    public event EventHandler<OnFactorGeneratorEventArgs>? OnFactorGenerator;

    /// <summary>
    /// 乘法产生式的事件
    /// </summary>
    public event EventHandler<OnMultiplyGeneratorEventArgs>? OnMultiplyGenerator;

    private PascalType? _termType;

    public PascalType TermType
    {
        get
        {
            if (_termType is null)
            {
                throw new InvalidOperationException();
            }

            return _termType;
        }
        set
        {
            _termType = value;
        }
    }

    public static Term Create(List<SyntaxNodeBase> children)
    {
        return new Term { Children = children };
    }

    private void RaiseEvent()
    {
        if (Children.Count == 1)
        {
            OnFactorGenerator?.Invoke(this, new OnFactorGeneratorEventArgs
            {
                Factor = Children[0].Convert<Factor>()
            });
        }
        else
        {
            OnMultiplyGenerator?.Invoke(this, new OnMultiplyGeneratorEventArgs
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
