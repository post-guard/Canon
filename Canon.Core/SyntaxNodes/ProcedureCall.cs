using Canon.Core.Abstractions;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;
using Canon.Core.SemanticParser;

namespace Canon.Core.SyntaxNodes;

public class ParameterGeneratorEventArgs : EventArgs
{
    public required ExpressionList Parameters { get; init; }
}

public class NoParameterGeneratorEventArgs : EventArgs;

public class ProcedureCall : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.ProcedureCall;

    public IdentifierSemanticToken ProcedureId
        => Children[0].Convert<TerminatedSyntaxNode>().Token.Convert<IdentifierSemanticToken>();

    /// <summary>
    /// 调用函数时含有参数的事件
    /// </summary>
    public event EventHandler<ParameterGeneratorEventArgs>? OnParameterGenerator;

    /// <summary>
    /// 调用函数是没有参数的事件
    /// </summary>
    public event EventHandler<NoParameterGeneratorEventArgs>? OnNoParameterGenerator;

    private PascalType? _pascalType;

    public PascalType ReturnType
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

    public static ProcedureCall Create(List<SyntaxNodeBase> children)
    {
        return new ProcedureCall { Children = children };
    }

    private void RaiseEvent()
    {
        if (Children.Count == 4)
        {
            OnParameterGenerator?.Invoke(this, new ParameterGeneratorEventArgs
            {
                Parameters = Children[2].Convert<ExpressionList>()
            });
        }
        else
        {
            OnNoParameterGenerator?.Invoke(this, new NoParameterGeneratorEventArgs());
        }

        OnParameterGenerator = null;
        OnNoParameterGenerator = null;
    }
}
