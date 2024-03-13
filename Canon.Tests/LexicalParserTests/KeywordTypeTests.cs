using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Tests.LexicalParserTests;

public class KeywordTypeTests
{
    [Theory]
    [InlineData("program", KeywordType.Program)]
    [InlineData("const", KeywordType.Const)]
    [InlineData("var", KeywordType.Var)]
    [InlineData("procedure", KeywordType.Procedure)]
    [InlineData("function", KeywordType.Function)]
    [InlineData("begin", KeywordType.Begin)]
    [InlineData("end", KeywordType.End)]
    [InlineData("array", KeywordType.Array)]
    [InlineData("of", KeywordType.Of)]
    [InlineData("if", KeywordType.If)]
    [InlineData("then", KeywordType.Then)]
    [InlineData("else", KeywordType.Else)]
    [InlineData("for", KeywordType.For)]
    [InlineData("to", KeywordType.To)]
    [InlineData("do", KeywordType.Do)]
    public void SmokeTest(string input, KeywordType type)
    {
        LinkedList<char> content = Utils.GetLinkedList(input);

        Assert.True(KeywordSemanticToken.TryParse(0, 0, content.First!,
            out KeywordSemanticToken? token));
        Assert.NotNull(token);
        Assert.Equal(type, token.KeywordType);
    }
}
