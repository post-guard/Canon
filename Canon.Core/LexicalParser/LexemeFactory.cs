using Canon.Core.Enums;

namespace Canon.Core.LexicalParser;

public static class LexemeFactory
{
    public static SemanticToken MakeToken(SemanticTokenType tokenType,string literal,uint line,uint chPos)
    {
        SemanticToken? token;
        switch (tokenType)
        {
            case SemanticTokenType.Character:
                CharacterSemanticToken characterSemanticToken = new()
                {
                    LinePos = line, CharacterPos = chPos, LiteralValue = literal,
                };
                token = characterSemanticToken;
                break;
            case SemanticTokenType.String:
                StringSemanticToken stringSemanticToken = new()
                {
                    LinePos = line, CharacterPos = chPos, LiteralValue = literal,
                };
                token = stringSemanticToken;
                break;
            case SemanticTokenType.Identifier:
                IdentifierSemanticToken identifierSemanticToken = new()
                {
                    LinePos = line, CharacterPos = chPos, LiteralValue = literal,
                };
                token = identifierSemanticToken;
                break;
            default:
                throw new InvalidOperationException("Can only create Character or Identifier SemanticToken.");
        }

        return token;
    }

    public static KeywordSemanticToken MakeToken(KeywordType keywordType,string literal,uint line,uint chPos)
    {
        KeywordSemanticToken keywordSemanticToken = new()
        {
            LinePos = line,
            CharacterPos = chPos,
            LiteralValue = literal,
            KeywordType = keywordType
        };
        return keywordSemanticToken;
    }

    public static DelimiterSemanticToken MakeToken(DelimiterType delimiterType,string literal,uint line,uint chPos)
    {
        DelimiterSemanticToken delimiterSemanticToken = new()
        {
            LinePos = line,
            CharacterPos = chPos,
            LiteralValue = literal,
            DelimiterType = delimiterType
        };
        return delimiterSemanticToken;
    }

    public static NumberSemanticToken MakeToken(NumberType numberType,string literal,uint line,uint chPos)
    {
        string temp = literal;
        string result;
        if (numberType == NumberType.Hex)
        {
            result = string.Concat("0x", temp.AsSpan(1, temp.Length - 1));
        }
        else
        {
            result = temp;
        }

        NumberSemanticToken numberSemanticToken = new()
        {
            LinePos = line,
            CharacterPos = chPos,
            LiteralValue = result,
            NumberType = numberType
        };
        return numberSemanticToken;

    }

    public static OperatorSemanticToken MakeToken(OperatorType operatorType,string literal,uint line,uint chPos)
    {
        OperatorSemanticToken operatorSemanticToken = new()
        {
            LinePos = line,
            CharacterPos = chPos,
            LiteralValue = literal,
            OperatorType = operatorType
        };
        return operatorSemanticToken;
    }
}
