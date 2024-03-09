namespace Canon.Core.LexicalParser;

using Enums;

public abstract class SemanticToken
{
    public abstract SemanticTokenType TokenType { get; }

    public required uint LinePos { get; init; }

    public required uint CharacterPos { get; init; }

    public required string LiteralValue { get; init; }
}

public class CharacterSemanticToken : SemanticToken
{
    public override SemanticTokenType TokenType => SemanticTokenType.Character;

    public static bool TryParse(uint linePos, uint characterPos, LinkedListNode<char> now, out CharacterSemanticToken? token)
    {
        token = null;
        return false;
    }
}

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
            { ']', DelimiterType.RightSquareBracket }
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
            // As there is now keyword shorter than 2 characters.
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

public class OperatorSemanticToken : SemanticToken
{
    public override SemanticTokenType TokenType => SemanticTokenType.Operator;

    public static bool TryParse(uint linePos, uint characterPos, LinkedListNode<char> now,
        out OperatorSemanticToken? token)
    {
        token = null;
        return false;
    }
}

public class NumberSemanticToken : SemanticToken
{
    public override SemanticTokenType TokenType => SemanticTokenType.Number;

    public static bool TryParse(uint linePos, uint characterPos, LinkedListNode<char> now,
        out NumberSemanticToken? token)
    {
        token = null;
        return false;
    }
}

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
