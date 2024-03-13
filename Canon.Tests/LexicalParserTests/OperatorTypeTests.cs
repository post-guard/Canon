using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Tests.LexicalParserTests;

public class OperatorTypeTests
{
    [Theory]
    [InlineData("+ 123", OperatorType.Plus)]
    [InlineData("1 + 123", OperatorType.Plus)]
    [InlineData("+123", OperatorType.Plus)]
    [InlineData("m +123", OperatorType.Plus)]
    [InlineData("-123", OperatorType.Minus)]
    [InlineData("*123", OperatorType.Multiply)]
    [InlineData("/123", OperatorType.Divide)]
    [InlineData("=123", OperatorType.Equal)]
    [InlineData("<123", OperatorType.Less)]
    [InlineData(">123", OperatorType.Greater)]
    [InlineData("<=123", OperatorType.LessEqual)]
    [InlineData(">=123", OperatorType.GreaterEqual)]
    [InlineData("<>123", OperatorType.NotEqual)]
    [InlineData(":=123", OperatorType.Assign)]
    public void ParseTest(string input, OperatorType result)
    {
        LinkedList<char> content = Utils.GetLinkedList(input);
        Assert.True(OperatorSemanticToken.TryParse(0, 0,
            content.First!, out OperatorSemanticToken? token));
        Assert.Equal(result, token?.OperatorType);
    }

    [Theory]
    [InlineData("<><123")]
    [InlineData("<=<123")]
    public void ParseFailedTest(string input)
    {
        LinkedList<char> content = Utils.GetLinkedList(input);
        Assert.False(OperatorSemanticToken.TryParse(0, 0,
            content.First!, out OperatorSemanticToken? token));
        Assert.Null(token);
    }
}
