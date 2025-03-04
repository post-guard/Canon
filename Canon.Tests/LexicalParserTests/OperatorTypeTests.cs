﻿using Canon.Core.Enums;
using Canon.Core.LexicalParser;
using Canon.Tests.Utils;
using Canon.Core.Abstractions;
namespace Canon.Tests.LexicalParserTests;

public class OperatorTypeTests
{
    private readonly ILexer _lexer = new Lexer();

    [Theory]
    [InlineData("+ 123", OperatorType.Plus, true)]
    [InlineData("+123", OperatorType.Plus, true)]
    [InlineData("-123", OperatorType.Minus, true)]
    [InlineData("*123", OperatorType.Multiply, true)]
    [InlineData("/123", OperatorType.Divide, true)]
    [InlineData("=123", OperatorType.Equal, true)]
    [InlineData("<123", OperatorType.Less, true)]
    [InlineData(">123", OperatorType.Greater, true)]
    [InlineData("<=123", OperatorType.LessEqual, true)]
    [InlineData(">=123", OperatorType.GreaterEqual, true)]
    [InlineData("<>123", OperatorType.NotEqual, true)]
    [InlineData(":=123", OperatorType.Assign, true)]
    [InlineData("1 + 123", OperatorType.Plus, false)]
    [InlineData("m +123", OperatorType.Plus, false)]
    public void ParseTest(string input, OperatorType result, bool expectedResult)
    {
        IEnumerable<SemanticToken> tokensEnumerable = _lexer.Tokenize(new StringSourceReader(input));
        List<SemanticToken> tokens = tokensEnumerable.ToList();

        SemanticToken token = tokens[0];
        if (!expectedResult)
        {
            Assert.NotEqual(SemanticTokenType.Operator, token.TokenType);
            return;
        }
        Assert.Equal(SemanticTokenType.Operator, token.TokenType);
        OperatorSemanticToken operatorSemanticToken = (OperatorSemanticToken)token;
        Assert.Equal(result, operatorSemanticToken.OperatorType);
    }
}
