using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Tests.LexicalParserTests;


public class DelimiterTests
{
    [Theory]
    [InlineData(",123", DelimiterType.Comma)]
    [InlineData(".123", DelimiterType.Period)]
    [InlineData(":123", DelimiterType.Colon)]
    [InlineData(";123", DelimiterType.Semicolon)]
    [InlineData("(123)", DelimiterType.LeftParenthesis)]
    [InlineData(").", DelimiterType.RightParenthesis)]
    [InlineData("[asd", DelimiterType.LeftSquareBracket)]
    [InlineData("]asd", DelimiterType.RightSquareBracket)]
    public void SmokeTest(string input, DelimiterType type)
    {
        LinkedList<char> content = Utils.GetLinkedList(input);

        Assert.True(DelimiterSemanticToken.TryParse(0, 0, content.First!,
            out DelimiterSemanticToken? token));
        Assert.NotNull(token);
        Assert.Equal(type, token.DelimiterType);
    }
}
