using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Tests.LexicalParserTests;

public class OperatorTypeTests
{
    [Theory]
    [InlineData("+ 123", OperatorType.Plus, 0u, 0u, true)]
    [InlineData("1 + 123", OperatorType.Plus, 0u, 2u, true)]
    [InlineData("+123", OperatorType.Plus, 0u, 0u, true)]
    [InlineData("m +123", OperatorType.Plus, 0u, 2u, true)]
    [InlineData("-123", OperatorType.Minus, 0u, 0u, true)]
    [InlineData("*123", OperatorType.Multiply, 0u, 0u, true)]
    [InlineData("/123", OperatorType.Divide, 0u, 0u, true)]
    [InlineData("=123", OperatorType.Equal, 0u, 0u, true)]
    [InlineData("<123", OperatorType.Less, 0u, 0u, true)]
    [InlineData(">123", OperatorType.Greater, 0u, 0u, true)]
    [InlineData("<=123", OperatorType.LessEqual, 0u, 0u, true)]
    [InlineData(">=123", OperatorType.GreaterEqual, 0u, 0u, true)]
    [InlineData("<>123", OperatorType.NotEqual, 0u, 0u, true)]
    [InlineData(":=123", OperatorType.Assign, 0u, 0u, true)]
    [InlineData("and 123", OperatorType.And, 0u, 0u, true)]
    [InlineData("or123", OperatorType.Or, 0u, 0u, true)]
    [InlineData("mod123", OperatorType.Mod, 0u, 0u, true)]

    [InlineData("and123", OperatorType.And, 0u, 0u, false)]
    [InlineData("andasd", OperatorType.And, 0u, 0u, false)]
    [InlineData("andand", OperatorType.And, 0u, 0u, false)]
    [InlineData("<><123", OperatorType.NotEqual, 0u, 0u, false)]
    [InlineData("<><123", OperatorType.Less, 0u, 0u, false)]
    [InlineData("<=<123", OperatorType.LessEqual, 0u, 0u, false)]
    public void SmokeTest(string input, OperatorType type, uint expectedLinePos, uint expectedCharacterPos, bool expectedResult)
    {
        LinkedList<char> content = Utils.GetLinkedList(input);
        Assert.Equal(expectedResult, OperatorSemanticToken.TryParse(expectedLinePos, expectedCharacterPos, content.First!,
            out OperatorSemanticToken? token));
        if (expectedResult)
        {
            Assert.NotNull(token);
            Assert.Equal(type, token.OperatorType);
            Assert.Equal(expectedLinePos, token.LinePos);
            Assert.Equal(expectedCharacterPos, token.CharacterPos);
        }
        else
        {
            Assert.Null(token);
        }
    }
}
