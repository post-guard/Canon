using Canon.Core.Enums;
using Canon.Core.LexicalParser;
using Xunit.Abstractions;
using Canon.Core.Exceptions;
using Canon.Core.Abstractions;
using Canon.Tests.Utils;

namespace Canon.Tests.LexicalParserTests
{
    public class CharacterTypeTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILexer _lexer = new Lexer();
        public CharacterTypeTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Theory]
        [InlineData("'a'", "a")]
        [InlineData("'Hello, World!'", "Hello, World!")]

        public void TestCharacterType(string input, string? expectedResult)
        {
            IEnumerable<SemanticToken> tokensEnumerable = _lexer.Tokenize(new StringSourceReader(input));
            List<SemanticToken> tokens = tokensEnumerable.ToList();
            if (expectedResult == null)
            {
                Assert.Throws<LexemeException>(() => tokens);
            }
            else
            {
                _testOutputHelper.WriteLine(tokens[0].LiteralValue);
                Assert.Equal(SemanticTokenType.Character, tokens[0].TokenType);
                Assert.Equal(expectedResult, tokens[0].LiteralValue);
            }
        }

        [Theory]
        //[InlineData("'\\x'", 1, 2, LexemeException.LexemeErrorType.InvalidEscapeSequence)]
        [InlineData("'This is an unclosed string literal", 1, 35, LexemeErrorType.UnclosedStringLiteral)]
        [InlineData("'This", 1, 5, LexemeErrorType.UnclosedStringLiteral)]
        [InlineData("x @", 1, 3, LexemeErrorType.UnknownCharacterOrString)]
        //[InlineData("\"x\'", 1, 3, LexemeException.LexemeErrorType.UnclosedStringLiteral)]
        public void TestParseCharacterError(string input,  uint expectedLine, uint expectedCharPosition, LexemeErrorType expectedErrorType)
        {

            var ex = Assert.Throws<LexemeException>(() => _lexer.Tokenize(new StringSourceReader(input)).ToList());
            _testOutputHelper.WriteLine(ex.ToString());
            Assert.Equal(expectedErrorType, ex.ErrorType);
            Assert.Equal(expectedLine, ex.Line);
            Assert.Equal(expectedCharPosition, ex.CharPosition);
        }
    }
}
