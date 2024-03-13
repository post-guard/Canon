using Canon.Core.GrammarParser;

namespace Canon.Core.LexicalParser;

using Enums;

using System.Text;

/// <summary>
/// 词法记号基类
/// </summary>
public abstract class SemanticToken
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

    /// <summary>
    /// 栈底符号单例对象
    /// </summary>
    public static EndSemanticToken End => new()
    {
        LinePos = 0, CharacterPos = 0, LiteralValue = string.Empty
    };

    public override string ToString() => LiteralValue;
}

/// <summary>
/// 字符类型记号
/// </summary>
public class CharacterSemanticToken : SemanticToken
{
    public override SemanticTokenType TokenType => SemanticTokenType.Character;

    public static bool TryParse(uint linePos, uint characterPos, LinkedListNode<char> now,
        out CharacterSemanticToken? token)
    {
        token = null;
        return false;
    }
}

/// <summary>
/// 分隔符类型记号
/// </summary>
public class DelimiterSemanticToken : SemanticToken
{
    public override SemanticTokenType TokenType => SemanticTokenType.Delimiter;

    public required DelimiterType DelimiterType { get; init; }

    public static bool TryParse(uint linePos, uint characterPos, LinkedListNode<char> now,
        out DelimiterSemanticToken? token)
    {
        Dictionary<char, DelimiterType> delimiterMap = new()
        {
            { ',', DelimiterType.Comma },
            { '.', DelimiterType.Period },
            { ':', DelimiterType.Colon },
            { ';', DelimiterType.Semicolon },
            { '(', DelimiterType.LeftParenthesis },
            { ')', DelimiterType.RightParenthesis },
            { '[', DelimiterType.LeftSquareBracket },
            { ']', DelimiterType.RightSquareBracket },
            { '\'', DelimiterType.SingleQuotation },
            { '\"', DelimiterType.DoubleQuotation }
        };

        if (!delimiterMap.TryGetValue(now.Value, out DelimiterType value))
        {
            token = null;
            return false;
        }

        token = new DelimiterSemanticToken
        {
            LinePos = linePos,
            CharacterPos = characterPos,
            LiteralValue = new string([now.Value]),
            DelimiterType = value
        };
        return true;
    }
}

/// <summary>
/// 关键字类型记号
/// </summary>
public class KeywordSemanticToken : SemanticToken
{
    public override SemanticTokenType TokenType => SemanticTokenType.Keyword;

    public required KeywordType KeywordType { get; init; }

    public static bool TryParse(uint linePos, uint characterPos, LinkedListNode<char> now,
        out KeywordSemanticToken? token)
    {
        string buffer = new([now.Value]);

        if (now.Next is null)
        {
            // 没有比两个字符更短的关键字
            token = null;
            return false;
        }

        now = now.Next;
        buffer += now.Value;

        switch (buffer)
        {
            case "do":
                token = new KeywordSemanticToken
                {
                    LinePos = linePos,
                    CharacterPos = characterPos,
                    LiteralValue = "do",
                    KeywordType = KeywordType.Do
                };
                return true;
            case "Of":
                token = new KeywordSemanticToken
                {
                    LinePos = linePos,
                    CharacterPos = characterPos,
                    LiteralValue = "of",
                    KeywordType = KeywordType.Of
                };
                return true;
            case "If":
                token = new KeywordSemanticToken
                {
                    LinePos = linePos,
                    CharacterPos = characterPos,
                    LiteralValue = "if",
                    KeywordType = KeywordType.If
                };
                return true;
        }

        token = null;
        return false;
    }
}

/// <summary>
/// 操作数类型记号
/// </summary>
public class OperatorSemanticToken : SemanticToken
{
    public override SemanticTokenType TokenType => SemanticTokenType.Operator;

    public required OperatorType OperatorType { get; init; }

    public static bool TryParse(uint linePos, uint characterPos, LinkedListNode<char> now,
        out OperatorSemanticToken? token)
    {
        token = null;
        return false;
    }
}

/// <summary>
/// 数值类型记号
/// </summary>
/// TODO：进制表示（只有$1的十六进制表示）
public class NumberSemanticToken : SemanticToken
{
    public override SemanticTokenType TokenType => SemanticTokenType.Number;

    public required NumberType NumberType { get; init; }
    public double Value { get; private init; }

    public static bool TryParse(uint linePos, uint characterPos, LinkedListNode<char> now,
        out NumberSemanticToken? token)
    {
        StringBuilder buffer = new();

        bool hasDecimalPoint = false;
        bool hasExponent = false;
        bool hasMinusSign = false;

        while (now != null && (char.IsDigit(now.Value) || now.Value == '.' || now.Value == 'e' || now.Value == 'E' || now.Value == '-' || now.Value == '+'))
        {
            if (now.Value == '.')
            {
                if (hasDecimalPoint)
                {
                    break;
                }
                hasDecimalPoint = true;
            }

            if (now.Value == 'e' || now.Value == 'E')
            {
                if (hasExponent)
                {
                    break;
                }
                hasExponent = true;
            }

            if (now.Value == '-' || now.Value == '+')
            {
                if (hasMinusSign)
                {
                    break;
                }
                hasMinusSign = true;
            }

            buffer.Append(now.Value);
            now = now.Next;
        }

        if (double.TryParse(buffer.ToString(), out double value))
        {
            token = new NumberSemanticToken
            {
                LinePos = linePos,
                CharacterPos = characterPos,
                LiteralValue = buffer.ToString(),
                Value = value,
                NumberType = hasDecimalPoint || hasExponent ? NumberType.Real : NumberType.Integer
            };
            return true;
        }

        token = null;
        return false;
    }
}

/// <summary>
/// 标识符类型记号
/// </summary>
public class IdentifierSemanticToken : SemanticToken
{
    public override SemanticTokenType TokenType => SemanticTokenType.Identifier;

    public static bool TryParse(uint linePos, uint characterPos, LinkedListNode<char> now,
        out IdentifierSemanticToken? token)
    {
        token = null;
        return false;
    }
}

public class EndSemanticToken : SemanticToken
{
    public override SemanticTokenType TokenType => SemanticTokenType.End;
}
