using Canon.Core.Enums;
using Canon.Core.LexicalParser;
using Canon.Tests.Utils;
using Canon.Core.Abstractions;

namespace Canon.Tests.LexicalParserTests;

public class KeywordTypeTests
{
    private readonly ILexer _lexer = new Lexer();

    [Theory]
    [InlineData("program", KeywordType.Program)]
    [InlineData("const", KeywordType.Const)]
    [InlineData("var", KeywordType.Var)]
    [InlineData("procedure", KeywordType.Procedure)]
    [InlineData("function", KeywordType.Function)]
    [InlineData("begin", KeywordType.Begin)]
    [InlineData("end", KeywordType.End)]
    [InlineData("array", KeywordType.Array)]
    [InlineData("of", KeywordType.Of)]
    [InlineData("if", KeywordType.If)]
    [InlineData("then", KeywordType.Then)]
    [InlineData("else", KeywordType.Else)]
    [InlineData("for", KeywordType.For)]
    [InlineData("to", KeywordType.To)]
    [InlineData("do", KeywordType.Do)]
    [InlineData("DO", KeywordType.Do)]
    [InlineData("true", KeywordType.True)]
    [InlineData("false", KeywordType.False)]
    public void SmokeTest(string input, KeywordType type)
    {
        IEnumerable<SemanticToken> tokensEnumerable = _lexer.Tokenize(new StringSourceReader(input));
        List<SemanticToken> tokens = tokensEnumerable.ToList();

        SemanticToken token = tokens[0];
        Assert.Equal(SemanticTokenType.Keyword, token.TokenType);
        KeywordSemanticToken keywordSemanticToken = (KeywordSemanticToken)token;
        Assert.Equal(type, keywordSemanticToken.KeywordType);
    }
}
