using Canon.Core.Abstractions;
using Canon.Core.Enums;
using Canon.Core.SemanticParser;

namespace Canon.Core.SyntaxNodes;

public class IndexGeneratorEventArgs : EventArgs
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

    /// <summary>
    /// 数组左边界列表
    /// </summary>
    public List<int> LeftBounds = new();

    /// <summary>
    /// 索引中的表达式
    /// </summary>
    public List<Expression> Expressions { get; } = [];

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
    public event EventHandler<IndexGeneratorEventArgs>? OnIndexGenerator;

    public static IdentifierVarPart Create(List<SyntaxNodeBase> children)
    {
        IdentifierVarPart result = new() { Children = children };

        if (children.Count == 3)
        {
            ExpressionList expressionList = children[1].Convert<ExpressionList>();

            result.Expressions.AddRange(expressionList.Expressions);
        }

        return result;
    }

    private void RaiseEvent()
    {
        if (Children.Count == 3)
        {
            OnIndexGenerator?.Invoke(this, new IndexGeneratorEventArgs()
            {
                IndexParameters = Children[1].Convert<ExpressionList>()
            });
        }

        OnIndexGenerator = null;
    }
}
