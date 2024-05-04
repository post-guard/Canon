using Canon.Core.Enums;
using Canon.Core.LexicalParser;
using Xunit.Abstractions;
using Canon.Core.Exceptions;
using Canon.Core.Abstractions;
using Canon.Tests.Utils;

namespace Canon.Tests.LexicalParserTests
{
    public class CharacterTypeTests(ITestOutputHelper testOutputHelper)
    {
        private readonly ILexer _lexer = new Lexer();

        [Theory]
        [InlineData("'a'", 'a')]
        [InlineData("'+'", '+')]
        public void TestCharacterType(string input, char expectedResult)
        {
            IEnumerable<SemanticToken> tokensEnumerable = _lexer.Tokenize(new StringSourceReader(input));
            List<SemanticToken> tokens = tokensEnumerable.ToList();

            testOutputHelper.WriteLine(tokens[0].LiteralValue);
            Assert.Equal(SemanticTokenType.Character, tokens[0].TokenType);
            Assert.Equal(expectedResult, tokens[0].Convert<CharacterSemanticToken>().ParseAsCharacter());
        }

        [Theory]
        [InlineData("'Hello, world'", "Hello, world")]
        [InlineData("'asdfasdf'", "asdfasdf")]
        public void StringTypeTest(string input, string expect)
        {
            IEnumerable<SemanticToken> tokens = _lexer.Tokenize(new StringSourceReader(input));
            SemanticToken token = tokens.First();

            Assert.Equal(SemanticTokenType.String, token.TokenType);
            Assert.Equal(expect, token.Convert<StringSemanticToken>().ParseAsString());
        }

        [Theory]
        //[InlineData("'\\x'", 1, 2, LexemeException.LexemeErrorType.InvalidEscapeSequence)]
        [InlineData("'This is an unclosed string literal", 1, 35, LexemeErrorType.UnclosedStringLiteral)]
        [InlineData("'This", 1, 5, LexemeErrorType.UnclosedStringLiteral)]
        [InlineData("x @", 1, 3, LexemeErrorType.UnknownCharacterOrString)]
        //[InlineData("\"x\'", 1, 3, LexemeException.LexemeErrorType.UnclosedStringLiteral)]
        public void TestParseCharacterError(string input, uint expectedLine, uint expectedCharPosition,
            LexemeErrorType expectedErrorType)
        {
            var ex = Assert.Throws<LexemeException>(() => _lexer.Tokenize(new StringSourceReader(input)).ToList());
            testOutputHelper.WriteLine(ex.ToString());
            Assert.Equal(expectedErrorType, ex.ErrorType);
            Assert.Equal(expectedLine, ex.Line);
            Assert.Equal(expectedCharPosition, ex.CharPosition);
        }
    }
}
