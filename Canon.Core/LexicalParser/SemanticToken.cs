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

    public static readonly Dictionary<string, KeywordType> KeywordTypes =
        new Dictionary<string, KeywordType>(StringComparer.OrdinalIgnoreCase)
        {
            { "program", KeywordType.Program },
            { "const", KeywordType.Const },
            { "var", KeywordType.Var },
            { "procedure", KeywordType.Procedure },
            { "function", KeywordType.Function },
            { "begin", KeywordType.Begin },
            { "end", KeywordType.End },
            { "array", KeywordType.Array },
            { "of", KeywordType.Of },
            { "if", KeywordType.If },
            { "then", KeywordType.Then },
            { "else", KeywordType.Else },
            { "for", KeywordType.For },
            { "to", KeywordType.To },
            { "do", KeywordType.Do },
            { "integer", KeywordType.Integer },
            { "real", KeywordType.Real },
            { "boolean", KeywordType.Boolean },
            { "character", KeywordType.Character },
            { "div", KeywordType.Divide }, // 注意: Pascal 使用 'div' 而不是 '/'
            { "not", KeywordType.Not },
            { "mod", KeywordType.Mod },
            { "and", KeywordType.And },
            { "or", KeywordType.Or }
        };

    public static KeywordType GetKeywordTypeByKeyword(string keyword)
    {
        if (KeywordTypes.TryGetValue(keyword, out var keywordType))
        {
            return keywordType;
        }
        else
        {
            throw new ArgumentException($"Unknown keyword: {keyword}");
        }
    }

    public override int GetHashCode()
    {
        return base.GetHashCode() ^ this.KeywordType.GetHashCode();
    }
}

/// <summary>
/// 操作数类型记号
/// </summary>
public class OperatorSemanticToken : SemanticToken
{
    public override SemanticTokenType TokenType => SemanticTokenType.Operator;

    public required OperatorType OperatorType { get; init; }

    public static readonly Dictionary<string, OperatorType> OperatorTypes = new Dictionary<string, OperatorType>
    {
        { "=", OperatorType.Equal },
        { "<>", OperatorType.NotEqual },
        { "<", OperatorType.Less },
        { "<=", OperatorType.LessEqual },
        { ">", OperatorType.Greater },
        { ">=", OperatorType.GreaterEqual },
        { "+", OperatorType.Plus },
        { "-", OperatorType.Minus },
        { "*", OperatorType.Multiply },
        { "/", OperatorType.Divide },
        { ":=", OperatorType.Assign }
    };

    public static OperatorType GetOperatorTypeByOperator(string operatorSymbol)
    {
        if (OperatorTypes.TryGetValue(operatorSymbol, out var operatorType))
        {
            return operatorType;
        }
        else
        {
            throw new ArgumentException($"Unknown operator: {operatorSymbol}");
        }
    }

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

public class EndSemanticToken : SemanticToken
{
    public override SemanticTokenType TokenType => SemanticTokenType.End;
}
