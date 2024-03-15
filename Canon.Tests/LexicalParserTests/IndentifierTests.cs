using Canon.Core.Enums;
using Canon.Core.LexicalParser;
using Xunit;

namespace Canon.Tests.LexicalParserTests
{
    public class IdentifierTests
    {
        [Theory]
        [InlineData("identifier", true)]
        [InlineData("_identifier", true)]
        [InlineData("identifier123", true)]
        [InlineData("123identifier", false)]
        [InlineData("identifier_with_underscores", true)]
        [InlineData("IdentifierWithCamelCase", true)]
        [InlineData("identifier-with-hyphen", false)]
        [InlineData("identifier with spaces", false)]
        [InlineData("identifier_with_special_chars@#", false)]
        [InlineData("", false)]
        [InlineData(" ", false)]
        [InlineData("andand", false)]
        public void TestParseIdentifier(string input, bool expectedResult)
        {
            Lexer lexer = new(input);
            List<SemanticToken> tokens = lexer.Tokenize();

            Assert.Equal(expectedResult, tokens.FirstOrDefault()?.TokenType == SemanticTokenType.Identifier);
        }
    }
}
