using Canon.Core.Abstractions;
using Canon.Core.CodeGenerators;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;
using Canon.Core.SemanticParser;

namespace Canon.Core.SyntaxNodes;

public class OnNumberGeneratorEventArgs : EventArgs
{
    public required NumberSemanticToken Token { get; init; }
}

public class OnVariableGeneratorEventArgs : EventArgs
{
    public required Variable Variable { get; init; }
}

public class OnParethnesisGeneratorEventArgs : EventArgs
{
    public required Expression Expression { get; init; }
}

public class OnProcedureCallGeneratorEventArgs : EventArgs
{
    public required IdentifierSemanticToken ProcedureName { get; init; }

    public required ExpressionList Parameters { get; init; }
}

public class OnNotGeneratorEventArgs : EventArgs
{
    public required Factor Factor { get; init; }
}

public class OnUminusGeneratorEventArgs : EventArgs
{
    public required Factor Factor { get; init; }
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
    public event EventHandler<OnNumberGeneratorEventArgs>? OnNumberGenerator;

    /// <summary>
    /// 使用括号产生式的事件
    /// </summary>
    public event EventHandler<OnParethnesisGeneratorEventArgs>? OnParethnesisGenerator;

    /// <summary>
    /// 使用变量产生式的事件
    /// </summary>
    public event EventHandler<OnVariableGeneratorEventArgs>? OnVariableGenerator;

    /// <summary>
    /// 使用过程调用产生式的事件
    /// </summary>
    public event EventHandler<OnProcedureCallGeneratorEventArgs>? OnProcedureCallGenerator;

    /// <summary>
    /// 使用否定产生式的事件
    /// </summary>
    public event EventHandler<OnNotGeneratorEventArgs>? OnNotGenerator;

    /// <summary>
    /// 使用负号产生式的事件
    /// </summary>
    public event EventHandler<OnUminusGeneratorEventArgs>? OnUminusGenerator;

    private PascalType? _factorType;

    public PascalType FactorType
    {
        get
        {
            if (_factorType is null)
            {
                throw new InvalidOperationException();
            }

            return _factorType;
        }
        set
        {
            _factorType = value;
        }
    }

    public static Factor Create(List<SyntaxNodeBase> children)
    {
        return new Factor { Children = children };
    }

    private void RaiseEvent()
    {
        if (Children.Count == 1)
        {
            //factor -> num
            if (Children[0].IsTerminated)
            {
                SemanticToken token = Children[0].Convert<TerminatedSyntaxNode>().Token;
                OnNumberGenerator?.Invoke(this,
                    new OnNumberGeneratorEventArgs { Token = token.Convert<NumberSemanticToken>() });
            }
            // factor -> variable
            else
            {
                OnVariableGenerator?.Invoke(this,
                    new OnVariableGeneratorEventArgs { Variable = Children[0].Convert<Variable>() });
            }
        }
        //factor -> ( expression )
        else if (Children.Count == 3)
        {
            OnParethnesisGenerator?.Invoke(this,
                new OnParethnesisGeneratorEventArgs { Expression = Children[1].Convert<Expression>() });
        }
        //factor -> id ( expression )
        else if (Children.Count == 4)
        {
            OnProcedureCallGenerator?.Invoke(this,
                new OnProcedureCallGeneratorEventArgs
                {
                    ProcedureName =
                        Children[0].Convert<TerminatedSyntaxNode>().Token.Convert<IdentifierSemanticToken>(),
                    Parameters = Children[2].Convert<ExpressionList>()
                });
        }
        else
        {
            SemanticToken token = Children[0].Convert<TerminatedSyntaxNode>().Token;
            Factor factor = Children[1].Convert<Factor>();

            if (token.TokenType == SemanticTokenType.Keyword)
            {
                // factor -> not factor
                OnNotGenerator?.Invoke(this, new OnNotGeneratorEventArgs { Factor = factor });
            }
            else
            {
                // factor -> uminus factor
                OnUminusGenerator?.Invoke(this, new OnUminusGeneratorEventArgs { Factor = factor });
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
