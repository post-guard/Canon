using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Tests.LexicalParserTests;

public class OperatorTypeTests
{
    [Theory]
    [InlineData("+ 123", OperatorType.Plus)]
    [InlineData("+123", OperatorType.Plus)]
    [InlineData("-123", OperatorType.Minus)]
    [InlineData("*123", OperatorType.Multiply)]
    [InlineData("/123", OperatorType.Divide)]
    [InlineData("=123", OperatorType.Equal)]
    [InlineData("<123", OperatorType.Less)]
    [InlineData(">123", OperatorType.Greater)]
    [InlineData("<=123", OperatorType.LessEqual)]
    [InlineData(">=123", OperatorType.GreaterEqual)]
    [InlineData("<>123", OperatorType.NotEqual)]
    [InlineData(":=123", OperatorType.Assign)]
    public void ParseTest(string input, OperatorType result)
    {
        Lexer lexer = new(input);
        List<SemanticToken> tokens = lexer.Tokenize();

        SemanticToken token = tokens[0];
        Assert.Equal(SemanticTokenType.Operator, token.TokenType);
        OperatorSemanticToken operatorSemanticToken = (OperatorSemanticToken)token;
        Assert.Equal(result, operatorSemanticToken.OperatorType);
    }

    [Theory]
    [InlineData("1 + 123")]
    [InlineData("m +123")]
    public void ParseFailedTest(string input)
    {
        Lexer lexer = new(input);
        List<SemanticToken> tokens = lexer.Tokenize();

        SemanticToken token = tokens[0];
        Assert.NotEqual(SemanticTokenType.Operator, token.TokenType);
    }
}
