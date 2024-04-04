using Canon.Core.Enums;
using Canon.Core.LexicalParser;
using Xunit.Abstractions;
using Canon.Core.Exceptions;

namespace Canon.Tests.LexicalParserTests
{
    public class CharacterTypeTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public CharacterTypeTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Theory]
        [InlineData("'a'", "a")]
        [InlineData("'Hello, World!'", "Hello, World!")]

        public void TestCharacterType(string input, string? expectedResult)
        {
            Lexer lexer = new(input);
            if (expectedResult == null)
            {
                Assert.Throws<LexemeException>(() => lexer.Tokenize());
            }
            else
            {
                List<SemanticToken> tokens = lexer.Tokenize();
                _testOutputHelper.WriteLine(tokens[0].LiteralValue);
                Assert.Single(tokens);
                Assert.Equal(SemanticTokenType.Character, tokens[0].TokenType);
                Assert.Equal(expectedResult, tokens[0].LiteralValue);
            }
        }

        [Theory]
        //[InlineData("'\\x'", 1, 2, LexemeException.LexemeErrorType.InvalidEscapeSequence)]
        [InlineData("'This is an unclosed string literal", 1, 36, LexemeErrorType.UnclosedStringLiteral)]
        [InlineData("'This", 1, 6, LexemeErrorType.UnclosedStringLiteral)]
        [InlineData("x @", 1, 3, LexemeErrorType.UnknownCharacterOrString)]
        //[InlineData("\"x\'", 1, 3, LexemeException.LexemeErrorType.UnclosedStringLiteral)]
        public void TestParseCharacterError(string input,  uint expectedLine, uint expectedCharPosition, LexemeErrorType expectedErrorType)
        {
            Lexer lexer = new(input);
            var ex = Assert.Throws<LexemeException>(() => lexer.Tokenize());
            _testOutputHelper.WriteLine(ex.ToString());
            Assert.Equal(expectedErrorType, ex.ErrorType);
            Assert.Equal(expectedLine, ex.Line);
            Assert.Equal(expectedCharPosition, ex.CharPosition);
        }
    }
}
