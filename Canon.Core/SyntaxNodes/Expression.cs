using Canon.Core.Abstractions;
using Canon.Core.CodeGenerators;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;
using Canon.Core.SemanticParser;

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

    public bool IsParam;    //是否为传参

    public bool ReferenceParam; //是否为引用传参

    public bool LastParam;  //是否为传参列表里最后一个参数
    /// <summary>
    /// 是否为数组下标
    /// </summary>
    public bool IsIndex { get; set; }

    /// <summary>
    /// 当前表达式对应的数组下标维度的左边界
    /// </summary>
    public int LeftBound;
    public bool IsForConditionBegin { get; set; }
    public bool IsForConditionEnd { get; set; }
    public bool IsAssign { get; set; }

    private IdentifierSemanticToken? _iterator;

    public IdentifierSemanticToken Iterator
    {
        get
        {
            if (_iterator is null)
            {
                throw new InvalidOperationException();
            }

            return _iterator;
        }
        set
        {
            _iterator = value;
        }
    }

    /// <summary>
    /// 直接赋值产生式的事件
    /// </summary>
    public event EventHandler<SimpleExpressionGeneratorEventArgs>? OnSimpleExpressionGenerator;

    /// <summary>
    /// 关系产生式的事件
    /// </summary>
    public event EventHandler<RelationGeneratorEventArgs>? OnRelationGenerator;

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
