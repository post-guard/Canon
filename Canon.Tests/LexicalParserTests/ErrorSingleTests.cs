using Canon.Core.LexicalParser;
using Canon.Core.Exceptions;
using Xunit.Abstractions;
using Canon.Core.Enums;
using Canon.Core.Abstractions;
using Canon.Tests.Utils;

namespace Canon.Tests.LexicalParserTests
{
    public class ErrorSingleTests
    {
        private readonly ILexer _lexer = new Lexer();
        private readonly ITestOutputHelper _testOutputHelper;
        public ErrorSingleTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Theory]
        [InlineData("program main; var a: integer; begin a := 3#; end.", 1, 43, LexemeErrorType.IllegalNumberFormat)]
        [InlineData("char c = 'abc;", 1, 15, LexemeErrorType.UnclosedStringLiteral)]
        [InlineData("x := 10 @;", 1, 9, LexemeErrorType.UnknownCharacterOrString)]
        [InlineData("identifier_with_special_chars@#",1, 30, LexemeErrorType.UnknownCharacterOrString)]
        public void TestUnknownCharacterError(string pascalProgram, uint expectedLine, uint expectedCharPosition, LexemeErrorType expectedErrorType)
        {
            var ex = Assert.Throws<LexemeException>(() => _lexer.Tokenize(new StringSourceReader(pascalProgram)).ToList());
            _testOutputHelper.WriteLine(ex.ToString());
            Assert.Equal(expectedErrorType, ex.ErrorType);
            Assert.Equal(expectedLine, ex.Line);
            Assert.Equal(expectedCharPosition, ex.CharPosition);
        }
    }
}
