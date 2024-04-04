using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Tests.LexicalParserTests
{
    public class IdentifierTests
    {
        [Theory]
        [InlineData("identifier", true)]
        [InlineData("_identifier", true)]
        [InlineData("identifier123", true)]
        [InlineData("identifier_with_underscores", true)]
        [InlineData("IdentifierWithCamelCase", true)]
        [InlineData("andand", true)]
        public void TestParseIdentifier(string input, bool expectedResult)
        {
            Lexer lexer = new(input);
            List<SemanticToken> tokens = lexer.Tokenize();

            Assert.Single(tokens);
            Assert.Equal(expectedResult, tokens.FirstOrDefault()?.TokenType == SemanticTokenType.Identifier);
        }
    }
}
