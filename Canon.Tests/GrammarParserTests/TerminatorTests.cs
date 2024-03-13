using Canon.Core.Enums;
using Canon.Core.GrammarParser;
using Canon.Core.LexicalParser;

namespace Canon.Tests.GrammarParserTests;

public class TerminatorTests
{
    [Fact]
    public void TerminatorInnerTest()
    {
        Terminator keywordTerminator1 = new(KeywordType.Array);
        Terminator keywordTerminator2 = new(KeywordType.Begin);

        Assert.False(keywordTerminator1 == keywordTerminator2);
        Assert.False(keywordTerminator1 == Terminator.CharacterTerminator);
        Assert.False(keywordTerminator2 == Terminator.IdentifierTerminator);

        Terminator keywordTerminator3 = new(KeywordType.Array);
        Assert.Equal(keywordTerminator1, keywordTerminator3);

        Terminator delimiterTerminator1 = new(DelimiterType.Colon);
        Assert.NotEqual(keywordTerminator1, delimiterTerminator1);
    }

    [Fact]
    public void TerminatorAndKeywordSemanticTokenTest()
    {
        Terminator keywordTerminator = new(KeywordType.Array);
        KeywordSemanticToken keywordSemanticToken = new()
        {
            LinePos = 0, CharacterPos = 0, KeywordType = KeywordType.Array, LiteralValue = "array"
        };
        Assert.True(keywordTerminator == keywordSemanticToken);
    }

    [Fact]
    public void TerminatorAndDelimiterSemanticTokenTest()
    {
        Terminator terminator = new(DelimiterType.Period);
        DelimiterSemanticToken token = new()
        {
            LinePos = 0, CharacterPos = 0, DelimiterType = DelimiterType.Period, LiteralValue = "."
        };
        Assert.True(token == terminator);
    }

    [Fact]
    public void TerminatorAndOperatorSemanticTokenTest()
    {
        Terminator terminator = new(OperatorType.GreaterEqual);
        OperatorSemanticToken token = new()
        {
            LinePos = 0, CharacterPos = 0, OperatorType = OperatorType.GreaterEqual, LiteralValue = ">="
        };
        Assert.True(token == terminator);
    }
}
