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
        [InlineData("andand",false)]
        public void TestParseIdentifier(string input, bool expectedResult)
        {
            LinkedList<char> content = Utils.GetLinkedList(input);
            Assert.Equal(expectedResult, IdentifierSemanticToken.TryParse(0, 0, content.First!,
                out IdentifierSemanticToken? token));
            if (expectedResult)
            {
                Assert.NotNull(token);
            }
            else
            {
                Assert.Null(token);
            }
        }
    }
}
