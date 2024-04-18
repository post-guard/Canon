using Canon.Core.Enums;
using Canon.Core.LexicalParser;
using Canon.Core.Exceptions;
using Xunit.Abstractions;
using Canon.Tests.Utils;
using Canon.Core.Abstractions;
namespace Canon.Tests.LexicalParserTests
{

    public class NumberTests
    {
        private readonly ILexer _lexer = new Lexer();
        private readonly ITestOutputHelper _testOutputHelper;
        public NumberTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Theory]
        [InlineData("123", "123", NumberType.Integer)]
        [InlineData("0", "0", NumberType.Integer)]
        [InlineData("1.23", "1.23", NumberType.Real)]
        [InlineData("0.0", "0.0", NumberType.Real)]
        [InlineData("1e7", "1e7", NumberType.Real)]
        [InlineData("1E7", "1E7", NumberType.Real)]
        [InlineData("1.23e-7", "1.23e-7", NumberType.Real)]
        [InlineData("1.23E-7", "1.23E-7", NumberType.Real)]
        [InlineData("1234567890", "1234567890", NumberType.Integer)]
        [InlineData("1234567890.1234567890", "1234567890.1234567890", NumberType.Real)]
        [InlineData("1e-7", "1e-7", NumberType.Real)]
        [InlineData("1E-7", "1E-7", NumberType.Real)]
        [InlineData(".67",".67", NumberType.Real)]
        [InlineData("$123", "0x123", NumberType.Hex)]
        public void TestParseNumber(string input, string expected, NumberType expectedNumberType)
        {
            IEnumerable<SemanticToken> tokensEnumerable = _lexer.Tokenize(new StringSourceReader(input));
            List<SemanticToken> tokens = tokensEnumerable.ToList();
            SemanticToken token = tokens[0];
            Assert.Equal(SemanticTokenType.Number, token.TokenType);
            NumberSemanticToken numberSemanticToken = (NumberSemanticToken)token;
            Assert.Equal(expectedNumberType, numberSemanticToken.NumberType);
            Assert.Equal(expected, numberSemanticToken.LiteralValue);
        }

        [Theory]
        [InlineData("1E",  1, 2, LexemeErrorType.IllegalNumberFormat)]
        [InlineData("123abc",  1, 4, LexemeErrorType.IllegalNumberFormat)]
        [InlineData("123.45.67",  1, 7, LexemeErrorType.IllegalNumberFormat)]
        [InlineData("123identifier", 1, 4, LexemeErrorType.IllegalNumberFormat)]
        public void TestParseNumberError(string input,  uint expectedLine, uint expectedCharPosition, LexemeErrorType expectedErrorType)
        {
            var ex = Assert.Throws<LexemeException>(() => _lexer.Tokenize(new StringSourceReader(input)).ToList());
            _testOutputHelper.WriteLine(ex.ToString());
            Assert.Equal(expectedErrorType, ex.ErrorType);
            Assert.Equal(expectedLine, ex.Line);
            Assert.Equal(expectedCharPosition, ex.CharPosition);
        }
    }
}
