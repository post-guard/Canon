using Canon.Core.Abstractions;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Core.SyntaxNodes;

public class OnParameterGeneratorEventArgs : EventArgs
{
    public required ExpressionList Parameters { get; init; }
}

public class ProcedureCall : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.ProcedureCall;

    public IdentifierSemanticToken ProcedureId
        => Children[0].Convert<TerminatedSyntaxNode>().Token.Convert<IdentifierSemanticToken>();

    /// <summary>
    /// 调用函数时含有参数的事件
    /// </summary>
    public event EventHandler<OnParameterGeneratorEventArgs>? OnParameterGenerator;

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
            OnParameterGenerator?.Invoke(this, new OnParameterGeneratorEventArgs
            {
                Parameters = Children[2].Convert<ExpressionList>()
            });
        }

        OnParameterGenerator = null;
    }
}
