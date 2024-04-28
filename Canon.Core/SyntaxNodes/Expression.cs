using Canon.Core.Abstractions;
using Canon.Core.CodeGenerators;
using Canon.Core.Enums;
using Canon.Core.SemanticParser;

namespace Canon.Core.SyntaxNodes;

public class OnSimpleExpressionGeneratorEventArgs : EventArgs
{
    public required SimpleExpression SimpleExpression { get; init; }
}

public class OnRelationGeneratorEventArgs : EventArgs
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
    /// 直接赋值产生式的事件
    /// </summary>
    public event EventHandler<OnSimpleExpressionGeneratorEventArgs>? OnSimpleExpressionGenerator;

    /// <summary>
    /// 关系产生式的事件
    /// </summary>
    public event EventHandler<OnRelationGeneratorEventArgs>? OnRelationGenerator;

    private PascalType? _expressionType;

    public PascalType ExpressionType
    {
        get
        {
            if (_expressionType is null)
            {
                throw new InvalidOperationException();
            }

            return _expressionType;
        }
        set
        {
            _expressionType = value;
        }
    }

    public static Expression Create(List<SyntaxNodeBase> children)
    {
        return new Expression { Children = children };
    }

    private void RaiseEvent()
    {
        if (Children.Count == 1)
        {
            OnSimpleExpressionGenerator?.Invoke(this, new OnSimpleExpressionGeneratorEventArgs
            {
                SimpleExpression = Children[0].Convert<SimpleExpression>()
            });
        }
        else
        {
            OnRelationGenerator?.Invoke(this, new OnRelationGeneratorEventArgs
            {
                Left = Children[0].Convert<SimpleExpression>(),
                Operator = Children[1].Convert<RelationOperator>(),
                Right = Children[2].Convert<SimpleExpression>()
            });
        }

        OnSimpleExpressionGenerator = null;
        OnRelationGenerator = null;
    }

    public override void GenerateCCode(CCodeBuilder builder)
    {
        foreach (var child in Children)
        {
            child.GenerateCCode(builder);
        }
    }
}
