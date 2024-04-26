using Canon.Core.Abstractions;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;
using Canon.Core.SemanticParser;

namespace Canon.Core.SyntaxNodes;

public class OnIdentifierGeneratorEventArgs : EventArgs
{
    public required IdentifierSemanticToken IdentifierToken { get; init; }

    public required IdentifierList IdentifierList { get; init; }
}

public class OnTypeGeneratorEventArgs : EventArgs
{
    public required TypeSyntaxNode TypeSyntaxNode { get; init; }
}

public class IdentifierList : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.IdentifierList;

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

    private PascalType? _definitionType;

    /// <summary>
    /// IdentifierList中定义的类型
    /// </summary>
    /// <exception cref="InvalidOperationException">尚未确定定义的类型</exception>
    public PascalType DefinitionType
    {
        get
        {
            if (_definitionType is null)
            {
                throw new InvalidOperationException();
            }

            return _definitionType;
        }
        set
        {
            _definitionType = value;
        }
    }

    /// <summary>
    /// 是否为参数中的引用参数
    /// </summary>
    public bool IsReference { get; set; }

    /// <summary>
    /// 是否在过程定义中使用
    /// </summary>
    public bool IsProcedure { get; set; }

    public event EventHandler<OnIdentifierGeneratorEventArgs>? OnIdentifierGenerator;

    public event EventHandler<OnTypeGeneratorEventArgs>? OnTypeGenerator;

    public static IdentifierList Create(List<SyntaxNodeBase> children)
    {
        return new IdentifierList { Children = children };
    }

    private void RaiseEvent()
    {
        if (Children.Count == 2)
        {
            OnTypeGenerator?.Invoke(this,
                new OnTypeGeneratorEventArgs { TypeSyntaxNode = Children[1].Convert<TypeSyntaxNode>() });
        }
        else
        {
            OnIdentifierGenerator?.Invoke(this, new OnIdentifierGeneratorEventArgs
            {
                IdentifierToken = Children[1].Convert<TerminatedSyntaxNode>().Token.Convert<IdentifierSemanticToken>(),
                IdentifierList = Children[2].Convert<IdentifierList>()
            });
        }

        OnTypeGenerator = null;
        OnIdentifierGenerator = null;
    }
}
