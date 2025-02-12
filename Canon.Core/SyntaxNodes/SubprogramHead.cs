﻿using Canon.Core.Abstractions;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Core.SyntaxNodes;

public class ProcedureGeneratorEventArgs : EventArgs;

public class FunctionGeneratorEventArgs : EventArgs
{
    public required BasicType ReturnType { get; init; }
}

public class SubprogramHead : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.SubprogramHead;

    /// <summary>
    /// 过程定义还是函数定义
    /// </summary>
    public bool IsProcedure { get; private init; }

    /// <summary>
    /// 子程序的名称
    /// </summary>
    public IdentifierSemanticToken SubprogramName =>
        Children[1].Convert<TerminatedSyntaxNode>().Token.Convert<IdentifierSemanticToken>();

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

    public event EventHandler<ProcedureGeneratorEventArgs>? OnProcedureGenerator;

    public event EventHandler<FunctionGeneratorEventArgs>? OnFunctionGenerator;

    public static SubprogramHead Create(List<SyntaxNodeBase> children)
    {
        bool isProcedure;

        TerminatedSyntaxNode node = children[0].Convert<TerminatedSyntaxNode>();
        KeywordSemanticToken token = (KeywordSemanticToken)node.Token;

        if (token.KeywordType == KeywordType.Procedure)
        {
            isProcedure = true;
        }
        else if (token.KeywordType == KeywordType.Function)
        {
            isProcedure = false;
        }
        else
        {
            throw new InvalidOperationException();
        }

        return new SubprogramHead { Children = children, IsProcedure = isProcedure };
    }

    private void RaiseEvent()
    {
        if (IsProcedure)
        {
            OnProcedureGenerator?.Invoke(this, new ProcedureGeneratorEventArgs());
        }
        else
        {
            OnFunctionGenerator?.Invoke(this,
                new FunctionGeneratorEventArgs { ReturnType = Children[4].Convert<BasicType>() });
        }

        OnProcedureGenerator = null;
        OnFunctionGenerator = null;
    }
}
