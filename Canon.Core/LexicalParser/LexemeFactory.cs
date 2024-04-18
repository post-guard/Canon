using Canon.Core.Enums;

namespace Canon.Core.LexicalParser;

public static class LexemeFactory
{

    public static SemanticToken MakeToken(SemanticTokenType tokenType,string literal,uint _line,uint _chPos)
    {
        SemanticToken? token;
        switch (tokenType)
        {
            case SemanticTokenType.Character:
                CharacterSemanticToken characterSemanticToken = new CharacterSemanticToken()
                {
                    LinePos = _line, CharacterPos = _chPos, LiteralValue = literal,
                };
                token = characterSemanticToken;
                break;
            case SemanticTokenType.Identifier:
                IdentifierSemanticToken identifierSemanticToken = new IdentifierSemanticToken()
                {
                    LinePos = _line, CharacterPos = _chPos, LiteralValue = literal,
                };
                token = identifierSemanticToken;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, null);
        }

        return token;


    }

    public static KeywordSemanticToken MakeToken(KeywordType keywordType,string literal,uint _line,uint _chPos)
    {
        KeywordSemanticToken keywordSemanticToken = new KeywordSemanticToken
        {
            LinePos = _line,
            CharacterPos = _chPos,
            LiteralValue = literal,
            KeywordType = keywordType
        };
        return keywordSemanticToken;
    }

    public static DelimiterSemanticToken MakeToken(DelimiterType delimiterType,string literal,uint _line,uint _chPos)
    {
        DelimiterSemanticToken delimiterSemanticToken = new DelimiterSemanticToken()
        {
            LinePos = _line,
            CharacterPos = _chPos,
            LiteralValue = literal,
            DelimiterType = delimiterType
        };
        return delimiterSemanticToken;
    }

    public static NumberSemanticToken MakeToken(NumberType numberType,string literal,uint _line,uint _chPos)
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

        NumberSemanticToken numberSemanticToken = new NumberSemanticToken()
        {
            LinePos = _line,
            CharacterPos = _chPos,
            LiteralValue = result,
            NumberType = numberType
        };
        return numberSemanticToken;

    }

    public static OperatorSemanticToken MakeToken(OperatorType operatorType,string literal,uint _line,uint _chPos)
    {
        OperatorSemanticToken operatorSemanticToken = new OperatorSemanticToken()
        {
            LinePos = _line,
            CharacterPos = _chPos,
            LiteralValue = literal,
            OperatorType = operatorType
        };
        return operatorSemanticToken;
    }
}
