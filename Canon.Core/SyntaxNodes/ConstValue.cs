using Canon.Core.Abstractions;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;
using Canon.Core.SemanticParser;

namespace Canon.Core.SyntaxNodes;

/// <summary>
/// 使用数值产生式事件的事件参数
/// </summary>
public class NumberConstValueEventArgs : EventArgs
{
    /// <summary>
    /// 是否含有负号
    /// </summary>
    public bool IsNegative { get; init; }

    /// <summary>
    /// 数值记号
    /// </summary>
    public required NumberSemanticToken Token { get; init; }
}

/// <summary>
/// 使用字符产生式事件的事件参数
/// </summary>
public class CharacterConstValueEventArgs : EventArgs
{
    /// <summary>
    /// 字符记号
    /// </summary>
    public required CharacterSemanticToken Token { get; init; }
}

public class ConstValue : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.ConstValue;

    public override void PreVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PreVisit(this);
        RaiseGeneratorEvent();
    }

    public override void PostVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PostVisit(this);
        RaiseGeneratorEvent();
    }

    private PascalType? _constType;

    /// <summary>
    /// 该ConstValue代表的类型
    /// </summary>
    /// <exception cref="InvalidOperationException">尚未分析该类型</exception>
    public PascalType ConstType
    {
        get
        {
            if (_constType is null)
            {
                throw new InvalidOperationException("ConstType has not been set");
            }

            return _constType;
        }
        set
        {
            _constType = value;
        }
    }

    public string ValueString { get; set; } = string.Empty;

    /// <summary>
    /// 使用数值产生式的事件
    /// </summary>
    public event EventHandler<NumberConstValueEventArgs>? OnNumberGenerator;

    /// <summary>
    /// 使用字符产生式的事件
    /// </summary>
    public event EventHandler<CharacterConstValueEventArgs>? OnCharacterGenerator;

    private void RaiseGeneratorEvent()
    {
        if (Children.Count == 2)
        {
            OperatorSemanticToken operatorSemanticToken = Children[0].Convert<TerminatedSyntaxNode>().Token
                .Convert<OperatorSemanticToken>();
            NumberSemanticToken numberSemanticToken = Children[1].Convert<TerminatedSyntaxNode>().Token
                .Convert<NumberSemanticToken>();

            OnNumberGenerator?.Invoke(this, new NumberConstValueEventArgs
            {
                Token = numberSemanticToken,
                IsNegative = operatorSemanticToken.OperatorType == OperatorType.Minus
            });

            return;
        }

        SemanticToken token = Children[0].Convert<TerminatedSyntaxNode>().Token;

        if (token.TokenType == SemanticTokenType.Number)
        {
            OnNumberGenerator?.Invoke(this,
                new NumberConstValueEventArgs
                {
                    Token = token.Convert<NumberSemanticToken>()
                });
        }
        else
        {
            OnCharacterGenerator?.Invoke(this, new CharacterConstValueEventArgs
            {
                Token = token.Convert<CharacterSemanticToken>()
            });
        }

        OnNumberGenerator = null;
        OnCharacterGenerator = null;
    }

    public static ConstValue Create(List<SyntaxNodeBase> children)
    {
        return new ConstValue { Children = children };
    }
}
