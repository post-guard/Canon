using Canon.Core.Abstractions;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Core.SyntaxNodes;

public class NumberGeneratorEventArgs : EventArgs
{
    public required NumberSemanticToken Token { get; init; }
}

public class VariableGeneratorEventArgs : EventArgs
{
    public required Variable Variable { get; init; }
}

public class ParethnesisGeneratorEventArgs : EventArgs
{
    public required Expression Expression { get; init; }
}

public class ProcedureCallGeneratorEventArgs : EventArgs
{
    public required IdentifierSemanticToken ProcedureName { get; init; }

    public List<Expression> Parameters { get; } = [];
}

public class NotGeneratorEventArgs : EventArgs
{
    public required Factor Factor { get; init; }
}

public class UminusGeneratorEventArgs : EventArgs
{
    public required Factor Factor { get; init; }
}

public class PlusGeneratorEventArgs : EventArgs
{
    public required Factor Factor { get; init; }
}

public class BooleanGeneratorEventArgs : EventArgs
{
    public required bool Value { get; init; }
}

public class Factor : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.Factor;

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
    /// 使用数值产生式的事件
    /// </summary>
    public event EventHandler<NumberGeneratorEventArgs>? OnNumberGenerator;

    /// <summary>
    /// 使用括号产生式的事件
    /// </summary>
    public event EventHandler<ParethnesisGeneratorEventArgs>? OnParethnesisGenerator;

    /// <summary>
    /// 使用变量产生式的事件
    /// </summary>
    public event EventHandler<VariableGeneratorEventArgs>? OnVariableGenerator;

    /// <summary>
    /// 使用否定产生式的事件
    /// </summary>
    public event EventHandler<NotGeneratorEventArgs>? OnNotGenerator;

    /// <summary>
    /// 使用负号产生式的事件
    /// </summary>
    public event EventHandler<UminusGeneratorEventArgs>? OnUminusGenerator;

    /// <summary>
    /// 使用加号产生式的事件
    /// </summary>
    public event EventHandler<PlusGeneratorEventArgs>? OnPlusGenerator;

    /// <summary>
    /// 使用布尔值产生式的事件
    /// </summary>
    public event EventHandler<BooleanGeneratorEventArgs>? OnBooleanGenerator;

    /// <summary>
    /// 过程调用产生式的事件
    /// </summary>
    public event EventHandler<ProcedureCallGeneratorEventArgs>? OnProcedureCallGenerator;

    public static Factor Create(List<SyntaxNodeBase> children)
    {
        return new Factor { Children = children };
    }

    private void RaiseEvent()
    {
        if (Children.Count == 1)
        {
            if (Children[0].IsTerminated)
            {
                SemanticToken token = Children[0].Convert<TerminatedSyntaxNode>().Token;

                switch (token.TokenType)
                {
                    // factor -> num
                    case SemanticTokenType.Number:
                        OnNumberGenerator?.Invoke(this,
                            new NumberGeneratorEventArgs { Token = token.Convert<NumberSemanticToken>() });
                        break;
                    // factor -> true | false
                    case SemanticTokenType.Keyword:
                        KeywordSemanticToken keywordSemanticToken = token.Convert<KeywordSemanticToken>();

                        switch (keywordSemanticToken.KeywordType)
                        {
                            case KeywordType.True:
                                OnBooleanGenerator?.Invoke(this, new BooleanGeneratorEventArgs { Value = true });
                                break;
                            case KeywordType.False:
                                OnBooleanGenerator?.Invoke(this, new BooleanGeneratorEventArgs { Value = false });
                                break;
                        }

                        break;
                }
            }
            else
            {
                OnVariableGenerator?.Invoke(this,
                    new VariableGeneratorEventArgs { Variable = Children[0].Convert<Variable>() });
            }
        }
        else if (Children.Count == 3)
        {
            TerminatedSyntaxNode terminatedSyntaxNode = Children[0].Convert<TerminatedSyntaxNode>();

            // factor -> ( expression )
            if (terminatedSyntaxNode.Token.TokenType == SemanticTokenType.Delimiter)
            {
                OnParethnesisGenerator?.Invoke(this,
                    new ParethnesisGeneratorEventArgs { Expression = Children[1].Convert<Expression>() });
            }
            else
            {
                // factor -> id ( )
                OnProcedureCallGenerator?.Invoke(this, new ProcedureCallGeneratorEventArgs
                {
                    ProcedureName = terminatedSyntaxNode.Token.Convert<IdentifierSemanticToken>()
                });
            }
        }
        else if (Children.Count == 4)
        {
            // factor -> id ( expressionList)
            ProcedureCallGeneratorEventArgs eventArgs = new()
            {
                ProcedureName =
                    Children[0].Convert<TerminatedSyntaxNode>().Token.Convert<IdentifierSemanticToken>(),
            };
            eventArgs.Parameters.AddRange(Children[2].Convert<ExpressionList>().Expressions);
            OnProcedureCallGenerator?.Invoke(this, eventArgs);
        }
        else
        {

            SemanticToken token = Children[0].Convert<TerminatedSyntaxNode>().Token;
            Factor factor = Children[1].Convert<Factor>();

            if (token.TokenType == SemanticTokenType.Keyword)
            {
                // factor -> not factor
                OnNotGenerator?.Invoke(this, new NotGeneratorEventArgs { Factor = factor });
            }
            else
            {
                OperatorSemanticToken operatorSemanticToken = token.Convert<OperatorSemanticToken>();

                switch (operatorSemanticToken.OperatorType)
                {
                    // factor -> + factor
                    case OperatorType.Plus:
                        OnPlusGenerator?.Invoke(this, new PlusGeneratorEventArgs { Factor = factor });
                        break;
                    // factor -> - factor
                    case OperatorType.Minus:
                        OnUminusGenerator?.Invoke(this, new UminusGeneratorEventArgs { Factor = factor });
                        break;
                }
            }
        }

        OnNumberGenerator = null;
        OnVariableGenerator = null;
        OnParethnesisGenerator = null;
        OnProcedureCallGenerator = null;
        OnNotGenerator = null;
        OnUminusGenerator = null;
    }
}
