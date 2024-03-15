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
        Lexer lexer = new(input);
        List<SemanticToken> tokens = lexer.Tokenize();

        SemanticToken token = tokens[0];
        Assert.Equal(SemanticTokenType.Delimiter, token.TokenType);
        DelimiterSemanticToken delimiterSemanticToken = (DelimiterSemanticToken)token;
        Assert.Equal(type, delimiterSemanticToken.DelimiterType);
    }
}
