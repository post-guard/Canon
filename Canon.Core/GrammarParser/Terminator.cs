using Canon.Core.Enums;

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
