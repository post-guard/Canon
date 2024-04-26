using Canon.Core.Abstractions;
using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class OnParameterGeneratorEventArgs : EventArgs
{
    public required ExpressionList Parameters { get; init; }
}

public class IdentifierVarPart : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.IdVarPart;

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

    public event EventHandler<OnParameterGeneratorEventArgs>? OnParameterGenerator;

    public static IdentifierVarPart Create(List<SyntaxNodeBase> children)
    {
        return new IdentifierVarPart { Children = children };
    }

    private void RaiseEvent()
    {
        if (Children.Count == 3)
        {
            OnParameterGenerator?.Invoke(this, new OnParameterGeneratorEventArgs
            {
                Parameters = Children[1].Convert<ExpressionList>()
            });
        }

        OnParameterGenerator = null;
    }
}
