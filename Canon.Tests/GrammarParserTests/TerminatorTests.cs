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
        LinkedList<char> keywordContent = Utils.GetLinkedList("array [3..9] of integer");

        Assert.True(KeywordSemanticToken.TryParse(0, 0, keywordContent.First!,
            out KeywordSemanticToken? keywordSemanticToken));
        Assert.NotNull(keywordSemanticToken);
        Assert.True(keywordTerminator == keywordSemanticToken);
    }

    [Fact]
    public void TerminatorAndDelimiterSemanticTokenTest()
    {
        Terminator terminator = new(DelimiterType.Period);
        LinkedList<char> content = Utils.GetLinkedList(".");

        Assert.True(DelimiterSemanticToken.TryParse(0, 0, content.First!,
            out DelimiterSemanticToken? token));
        Assert.NotNull(token);
        Assert.True(token == terminator);
    }

    [Fact]
    public void TerminatorAndOperatorSemanticTokenTest()
    {
        Terminator terminator = new(OperatorType.GreaterEqual);
        LinkedList<char> content = Utils.GetLinkedList(">=");

        Assert.True(OperatorSemanticToken.TryParse(0, 0, content.First!,
            out OperatorSemanticToken? token));
        Assert.NotNull(token);
        Assert.True(token == terminator);
    }

    [Fact]
    public void TerminatorAndNumberSemanticTokenTest()
    {
        LinkedList<char> content = Utils.GetLinkedList("123");

        Assert.True(NumberSemanticToken.TryParse(0, 0, content.First!,
            out NumberSemanticToken? token));
        Assert.NotNull(token);
        Assert.True(Terminator.NumberTerminator == token);
    }

    [Fact]
    public void TerminatorAndCharacterSemanticTokenTest()
    {
        LinkedList<char> content = Utils.GetLinkedList("'a'");

        Assert.True(CharacterSemanticToken.TryParse(0, 0, content.First!,
            out CharacterSemanticToken? token));
        Assert.NotNull(token);
        Assert.True(Terminator.CharacterTerminator == token);
    }

    [Fact]
    public void TerminatorAndIdentifierSemanticTokenTest()
    {
        LinkedList<char> content = Utils.GetLinkedList("gcd");

        Assert.True(IdentifierSemanticToken.TryParse(0, 0, content.First!,
            out IdentifierSemanticToken? token));
        Assert.NotNull(token);
        Assert.True(Terminator.IdentifierTerminator == token);
    }
}
