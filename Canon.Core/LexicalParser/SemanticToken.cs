using Canon.Core.GrammarParser;

namespace Canon.Core.LexicalParser;

using Enums;

/// <summary>
/// 词法记号基类
/// </summary>
public abstract class SemanticToken : IEquatable<SemanticToken>
{
    public abstract SemanticTokenType TokenType { get; }

    /// <summary>
    /// 记号出现的行号
    /// </summary>
    public required uint LinePos { get; init; }

    /// <summary>
    /// 记号出现的列号
    /// </summary>
    public required uint CharacterPos { get; init; }

    /// <summary>
    /// 记号的字面值
    /// </summary>
    public required string LiteralValue { get; init; }

    public static implicit operator Terminator(SemanticToken token)
    {
        switch (token.TokenType)
        {
            case SemanticTokenType.Character:
                return Terminator.CharacterTerminator;
            case SemanticTokenType.Identifier:
                return Terminator.IdentifierTerminator;
            case SemanticTokenType.Number:
                return Terminator.NumberTerminator;
            case SemanticTokenType.End:
                return Terminator.EndTerminator;
            case SemanticTokenType.Delimiter:
                return new Terminator(((DelimiterSemanticToken)token).DelimiterType);
            case SemanticTokenType.Keyword:
                return new Terminator(((KeywordSemanticToken)token).KeywordType);
            case SemanticTokenType.Operator:
                return new Terminator(((OperatorSemanticToken)token).OperatorType);
            default:
                throw new ArgumentException("Unknown token type");
        }
    }

    public T Convert<T>() where T : SemanticToken
    {
        if (this is T result)
        {
            return result;
        }

        throw new InvalidOperationException("Can not convert target type0");
    }

    /// <summary>
    /// 栈底符号单例对象
    /// </summary>
    public static EndSemanticToken End => new()
    {
        LinePos = uint.MaxValue, CharacterPos = uint.MaxValue, LiteralValue = string.Empty
    };

    public override string ToString()
    {
        return
            $"LinePos: {LinePos}, CharacterPos: {CharacterPos}, LiteralValue: {LiteralValue}, TokenType: {TokenType}";
    }

    public bool Equals(SemanticToken? other)
    {
        if (other == null)
            return false;

        return LinePos == other.LinePos &&
               CharacterPos == other.CharacterPos &&
               LiteralValue == other.LiteralValue &&
               TokenType == other.TokenType;
    }

    public override bool Equals(object? obj)
    {
        return obj is SemanticToken semanticTokenObj && Equals(semanticTokenObj);
    }

    public override int GetHashCode()
    {
        return LinePos.GetHashCode() ^
               CharacterPos.GetHashCode() ^
               LiteralValue.GetHashCode() ^
               TokenType.GetHashCode();
    }
}

/// <summary>
/// 字符类型记号
/// </summary>
public class CharacterSemanticToken : SemanticToken
{
    public override SemanticTokenType TokenType => SemanticTokenType.Character;

    /// <summary>
    /// 获得令牌代表的字符
    /// </summary>
    /// <returns>字符</returns>
    public char ParseAsCharacter()
    {
        return char.Parse(LiteralValue);
    }
}

/// <summary>
/// 字符串类型记号
/// </summary>
public class StringSemanticToken : SemanticToken
{
    public override SemanticTokenType TokenType => SemanticTokenType.String;

    /// <summary>
    /// 获得令牌代表的字符串
    /// </summary>
    /// <returns>字符串</returns>
    public string ParseAsString()
    {
        return LiteralValue;
    }
}

/// <summary>
/// 分隔符类型记号
/// </summary>
public class DelimiterSemanticToken : SemanticToken
{
    public override SemanticTokenType TokenType => SemanticTokenType.Delimiter;

    public required DelimiterType DelimiterType { get; init; }

    public override int GetHashCode()
    {
        return base.GetHashCode() ^ DelimiterType.GetHashCode();
    }
}

/// <summary>
/// 关键字类型记号
/// </summary>
public class KeywordSemanticToken : SemanticToken
{
    public override SemanticTokenType TokenType => SemanticTokenType.Keyword;

    public required KeywordType KeywordType { get; init; }

    public override int GetHashCode()
    {
        return base.GetHashCode() ^ KeywordType.GetHashCode();
    }
}

/// <summary>
/// 操作数类型记号
/// </summary>
public class OperatorSemanticToken : SemanticToken
{
    public override SemanticTokenType TokenType => SemanticTokenType.Operator;

    public required OperatorType OperatorType { get; init; }

    public override int GetHashCode()
    {
        return base.GetHashCode() ^ OperatorType.GetHashCode();
    }
}

/// <summary>
/// 数值类型记号
/// </summary>
public class NumberSemanticToken : SemanticToken
{
    public override SemanticTokenType TokenType => SemanticTokenType.Number;

    public required NumberType NumberType { get; init; }

    /// <summary>
    /// 将数值类型记号识别为整数
    /// </summary>
    /// <returns>该记号表示的整数</returns>
    /// <exception cref="InvalidOperationException">目标记号不是整数类型</exception>
    public int ParseAsInteger()
    {
        if (NumberType != NumberType.Integer)
        {
            throw new InvalidOperationException("Target semantic token isn't integer");
        }

        return int.Parse(LiteralValue);
    }

    /// <summary>
    /// 将数值类型记号识别为浮点数
    /// </summary>
    /// <returns>该记号标识的浮点数</returns>
    /// <exception cref="InvalidOperationException">目标记号不是浮点数类型</exception>
    public double ParseAsReal()
    {
        if (NumberType != NumberType.Real)
        {
            throw new InvalidOperationException("Target semantic token isn't real");
        }

        return double.Parse(LiteralValue);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode() ^ NumberType.GetHashCode();
    }
}

/// <summary>
/// 标识符类型记号
/// </summary>
public class IdentifierSemanticToken : SemanticToken
{
    public override SemanticTokenType TokenType => SemanticTokenType.Identifier;

    /// <summary>
    /// 标识符名称
    /// </summary>
    public string IdentifierName => LiteralValue.ToLower();
}

/// <summary>
/// 终结符记号
/// </summary>
public class EndSemanticToken : SemanticToken
{
    public override SemanticTokenType TokenType => SemanticTokenType.End;
}
