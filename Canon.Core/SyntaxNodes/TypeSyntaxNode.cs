using Canon.Core.Abstractions;
using Canon.Core.CodeGenerators;
using Canon.Core.Enums;
using Canon.Core.SemanticParser;

namespace Canon.Core.SyntaxNodes;

public class BasicTypeGeneratorEventArgs : EventArgs
{
    public required BasicType BasicType { get; init; }
}

public class ArrayTypeGeneratorEventArgs : EventArgs
{
    public required Period Period { get; init; }

    public required BasicType BasicType { get; init; }
}

public class TypeSyntaxNode : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.Type;

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

    public event EventHandler<BasicTypeGeneratorEventArgs>? OnBasicTypeGenerator;

    public event EventHandler<ArrayTypeGeneratorEventArgs>? OnArrayTypeGenerator;

    /// <summary>
    /// 是否在过程定义中使用
    /// </summary>
    public bool IsProcedure { get; set; }

    private PascalType? _pascalType;

    /// <summary>
    /// Type节点制定的类型
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public PascalType PascalType
    {
        get
        {
            if (_pascalType is null)
            {
                throw new InvalidOperationException();
            }

            return _pascalType;
        }
        set
        {
            _pascalType = value;
        }
    }

    public static TypeSyntaxNode Create(List<SyntaxNodeBase> children)
    {
        return new TypeSyntaxNode { Children = children };
    }

    private void RaiseEvent()
    {
        if (Children.Count == 1)
        {
            OnBasicTypeGenerator?.Invoke(this, new BasicTypeGeneratorEventArgs
            {
                BasicType = Children[0].Convert<BasicType>()
            });
        }
        else
        {
            OnArrayTypeGenerator?.Invoke(this, new ArrayTypeGeneratorEventArgs
            {
                Period = Children[2].Convert<Period>(),
                BasicType = Children[5].Convert<BasicType>()
            });
        }

        OnBasicTypeGenerator = null;
        OnArrayTypeGenerator = null;
    }
}
