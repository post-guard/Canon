using Canon.Core.Abstractions;
using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class OnIndexGeneratorEventArgs : EventArgs
{
    public required ExpressionList IndexParameters { get; init; }
}

public class IdentifierVarPart : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.IdVarPart;

    /// <summary>
    /// 数组索引的个数
    /// </summary>
    public int IndexCount { get; set; }

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
    /// 使用了索引产生式的事件
    /// </summary>
    public event EventHandler<OnIndexGeneratorEventArgs>? OnIndexGenerator;

    public static IdentifierVarPart Create(List<SyntaxNodeBase> children)
    {
        return new IdentifierVarPart { Children = children };
    }

    private void RaiseEvent()
    {
        if (Children.Count == 3)
        {
            OnIndexGenerator?.Invoke(this, new OnIndexGeneratorEventArgs()
            {
                IndexParameters = Children[1].Convert<ExpressionList>()
            });
        }

        OnIndexGenerator = null;
    }
}
