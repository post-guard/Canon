using Canon.Core.Enums;
using Canon.Core.LexicalParser;
using Canon.Tests.Utils;
using Canon.Core.Abstractions;
namespace Canon.Tests.LexicalParserTests
{
    public class IdentifierTests
    {
        private readonly ILexer _lexer = new Lexer();

        [Theory]
        [InlineData("identifier", true)]
        [InlineData("_identifier", true)]
        [InlineData("identifier123", true)]
        [InlineData("identifier_with_underscores", true)]
        [InlineData("IdentifierWithCamelCase", true)]
        [InlineData("andand", true)]
        public void TestParseIdentifier(string input, bool expectedResult)
        {
            IEnumerable<SemanticToken> tokensEnumerable = _lexer.Tokenize(new StringSourceReader(input));
            List<SemanticToken> tokens = tokensEnumerable.ToList();

            Assert.Equal(expectedResult, tokens.FirstOrDefault()?.TokenType == SemanticTokenType.Identifier);
        }
    }
}
