using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Core.GrammarParser;

public abstract class TerminatorBase
{
    public abstract bool IsTerminated { get; }
}

/// <summary>
/// 语法中的一个终结符
/// 终结符标识词法分析中得到的一个记号
/// </summary>
public class Terminator : TerminatorBase, IEquatable<Terminator>
{
    public override bool IsTerminated => true;

    private readonly SemanticTokenType _terminatorType;

    private readonly KeywordType _keywordType;
    private readonly DelimiterType _delimiterType;
    private readonly OperatorType _operatorType;

    public Terminator(KeywordType keywordType)
    {
        _terminatorType = SemanticTokenType.Keyword;
        _keywordType = keywordType;
    }

    public Terminator(DelimiterType delimiterType)
    {
        _terminatorType = SemanticTokenType.Delimiter;
        _delimiterType = delimiterType;
    }

    public Terminator(OperatorType operatorType)
    {
        _terminatorType = SemanticTokenType.Operator;
        _operatorType = operatorType;
    }

    private Terminator(SemanticTokenType type)
    {
        _terminatorType = type;
    }

    /// <summary>
    /// 标识符终结符单例
    /// 鉴于在语法中不关心标识符具体内容，因此可以使用单例对象
    /// </summary>
    public static Terminator IdentifierTerminator => new(SemanticTokenType.Identifier);

    /// <summary>
    /// 字符终结符单例
    /// 鉴于在语法中不关心具体字符，因此可以使用单例对象
    /// </summary>
    public static Terminator CharacterTerminator => new(SemanticTokenType.Character);

    /// <summary>
    /// 数值终结符单例
    /// 鉴于在语法中不关心具体数值，因此可以使用单例对象
    /// </summary>
    public static Terminator NumberTerminator => new(SemanticTokenType.Number);

    /// <summary>
    /// 栈底的终结符
    /// </summary>
    public static Terminator EndTerminator => new(SemanticTokenType.End);

    public override int GetHashCode()
    {
        int hash = _terminatorType.GetHashCode();

        switch (_terminatorType)
        {
            case SemanticTokenType.Keyword:
                return hash ^ _keywordType.GetHashCode();
            case SemanticTokenType.Delimiter:
                return hash ^ _delimiterType.GetHashCode();
            case SemanticTokenType.Operator:
                return hash ^ _operatorType.GetHashCode();
            default:
                return hash;
        }
    }

    public override string ToString()
    {
        switch (_terminatorType)
        {
            case SemanticTokenType.Keyword:
                return _keywordType.ToString();
            case SemanticTokenType.Operator:
                return _operatorType.ToString();
            case SemanticTokenType.Delimiter:
                return _delimiterType.ToString();
            default:
                return _keywordType.ToString();
        }
    }

    public bool Equals(Terminator? other)
    {
        if (other is null)
        {
            return false;
        }

        if (_terminatorType != other._terminatorType)
        {
            return false;
        }

        switch (_terminatorType)
        {
            case SemanticTokenType.Keyword:
                return _keywordType == other._keywordType;
            case SemanticTokenType.Delimiter:
                return _delimiterType == other._delimiterType;
            case SemanticTokenType.Operator:
                return _operatorType == other._operatorType;
            default:
                return true;
        }
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Terminator other)
        {
            return false;
        }

        return Equals(other);
    }

    public static bool operator ==(Terminator a, Terminator b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(Terminator a, Terminator b)
    {
        return !a.Equals(b);
    }

    public static bool operator ==(Terminator a, SemanticToken b)
    {
        return a.EqualSemanticToken(b);
    }

    public static bool operator !=(Terminator a, SemanticToken b)
    {
        return !a.EqualSemanticToken(b);
    }

    public static bool operator ==(SemanticToken a, Terminator b)
    {
        return b.EqualSemanticToken(a);
    }

    public static bool operator !=(SemanticToken a, Terminator b)
    {
        return !b.EqualSemanticToken(a);
    }

    private bool EqualSemanticToken(SemanticToken token)
    {
        if (token.TokenType != _terminatorType)
        {
            return false;
        }

        switch (_terminatorType)
        {
            case SemanticTokenType.Delimiter:
                return (token as DelimiterSemanticToken)?.DelimiterType == _delimiterType;
            case SemanticTokenType.Keyword:
                return (token as KeywordSemanticToken)?.KeywordType == _keywordType;
            case SemanticTokenType.Operator:
                return (token as OperatorSemanticToken)?.OperatorType == _operatorType;
        }

        return true;
    }
}

/// <summary>
/// 语法中的非终结符
/// </summary>
public class NonTerminator : TerminatorBase, IEquatable<NonTerminator>
{
    public override bool IsTerminated => false;

    private readonly NonTerminatorType _type;

    public NonTerminator(NonTerminatorType type)
    {
        _type = type;
    }

    public override int GetHashCode()
    {
        return _type.GetHashCode();
    }

    public override string ToString() => _type.ToString();

    public bool Equals(NonTerminator? other)
    {
        if (other is null)
        {
            return false;
        }

        return _type == other._type;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not NonTerminator other)
        {
            return false;
        }

        return Equals(other);
    }

    public static bool operator ==(NonTerminator a, NonTerminator b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(NonTerminator a, NonTerminator b)
    {
        return !a.Equals(b);
    }
}
