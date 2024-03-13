using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Tests.LexicalParserTests
{
    public class NumberTests
    {
        [Theory]
        [InlineData("123", 123, NumberType.Integer)]
        [InlineData("0", 0, NumberType.Integer)]
        [InlineData("-123", -123, NumberType.Integer)]
        [InlineData("1.23", 1.23, NumberType.Real)]
        [InlineData("-1.23", -1.23, NumberType.Real)]
        [InlineData("0.0", 0.0, NumberType.Real)]
        [InlineData("1e7", 1e7, NumberType.Real)]
        [InlineData("1E7", 1E7, NumberType.Real)]
        [InlineData("1.23e-7", 1.23e-7, NumberType.Real)]
        [InlineData("1.23E-7", 1.23E-7, NumberType.Real)]
        [InlineData("1234567890", 1234567890, NumberType.Integer)]
        [InlineData("1234567890.1234567890", 1234567890.1234567890, NumberType.Real)]
        [InlineData("-1234567890", -1234567890, NumberType.Integer)]
        [InlineData("-1234567890.1234567890", -1234567890.1234567890, NumberType.Real)]
        [InlineData("1e-7", 1e-7, NumberType.Real)]
        [InlineData("1E-7", 1E-7, NumberType.Real)]
        [InlineData("1E", 0, NumberType.Real, false)]
        [InlineData("abc", 0, NumberType.Integer, false)]
        [InlineData("123abc", 123, NumberType.Integer, true)]
        public void TestParseNumber(string input, double expected, NumberType expectedNumberType, bool expectedResult = true)
        {
            LinkedList<char> content = Utils.GetLinkedList(input);
            Assert.Equal(expectedResult, NumberSemanticToken.TryParse(0, 0, content.First!,
                out NumberSemanticToken? token));
            if (expectedResult)
            {
                Assert.NotNull(token);
                Assert.Equal(expected, token.Value);
                Assert.Equal(expectedNumberType, token.NumberType);
            }
            else
            {
                Assert.Null(token);
            }
        }
    }
}
