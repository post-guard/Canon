using System.Text.RegularExpressions;
using Canon.Core.Enums;
using Canon.Core.Exceptions;
using Canon.Core.LexicalParser;
using Xunit.Abstractions;

namespace Canon.Tests.LexicalParserTests;

public class LexicalFileTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public LexicalFileTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    //TODO: 基础的字符串匹配，因此变量名称不能被包含。手写一个存在包含情况的测试文件。
    private static (int, int) FindNthPosition(string pascalProgram, string target, int occurrence)
    {
        int lineNumber = 0;
        (int, int) nthPosition = (0, 0);
        int foundCount = 0;
        occurrence = occurrence + 1;

        using (StringReader sr = new StringReader(pascalProgram))
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                lineNumber++;
                int columnNumber = -1;

                // line = Regex.Replace(line, "'[^']*'", "$");

                while ((columnNumber = line.IndexOf(target, columnNumber + 1, StringComparison.Ordinal)) != -1)
                {
                    foundCount++;
                    if (foundCount == occurrence)
                    {
                        nthPosition = (lineNumber, columnNumber + target.Length);
                        return nthPosition;
                    }
                }
            }
        }

        if (nthPosition == (0, 0))
        {
            throw new Exception($"'{target}' not found in program.");
        }

        return nthPosition;
    }

    private void TestLexicalAnalysis(string pascalProgram, List<(string, SemanticTokenType, int)> stringLiterals)
    {
        var expectedTokens = new List<SemanticToken>();

        foreach (var (literal, tokenType, skipCount) in stringLiterals)
        {
            var (line, column) = FindNthPosition(pascalProgram, literal, skipCount);
            switch (tokenType)
            {
                case SemanticTokenType.Keyword:
                    expectedTokens.Add(new KeywordSemanticToken
                    {
                        LinePos = (uint)line,
                        CharacterPos = (uint)column,
                        LiteralValue = literal,
                        KeywordType = KeywordSemanticToken.GetKeywordTypeByKeyword(literal)
                    });
                    break;
                case SemanticTokenType.Identifier:
                    expectedTokens.Add(new IdentifierSemanticToken
                    {
                        LinePos = (uint)line, CharacterPos = (uint)column, LiteralValue = literal
                    });
                    break;
                case SemanticTokenType.Delimiter:
                    if (DelimiterSemanticToken.TryParse((uint)line, (uint)column, new LinkedListNode<char>(literal[0]),
                            out var delimiterToken))
                    {
                        if (delimiterToken != null)
                        {
                            expectedTokens.Add(delimiterToken);
                        }
                    }

                    break;
                case SemanticTokenType.Operator:
                    expectedTokens.Add(new OperatorSemanticToken
                    {
                        LinePos = (uint)line,
                        CharacterPos = (uint)column,
                        LiteralValue = literal,
                        OperatorType = OperatorSemanticToken.GetOperatorTypeByOperator(literal)
                    });
                    break;
                case SemanticTokenType.Character:
                    expectedTokens.Add(new CharacterSemanticToken
                    {
                        LinePos = (uint)line, CharacterPos = (uint)column, LiteralValue = literal
                    });
                    break;
                case SemanticTokenType.Number:
                    expectedTokens.Add(new NumberSemanticToken
                    {
                        LinePos = (uint)line,
                        CharacterPos = (uint)column,
                        LiteralValue = literal,
                        NumberType = NumberType.Integer
                    });
                    break;
            }
        }

        expectedTokens = expectedTokens.OrderBy(token => token.LinePos).ThenBy(token => token.CharacterPos).ToList();
        expectedTokens = expectedTokens.Select(token =>
            token is CharacterSemanticToken characterToken && characterToken.LiteralValue == "hello, world!"
                ? new CharacterSemanticToken
                {
                    LinePos = characterToken.LinePos,
                    CharacterPos = characterToken.CharacterPos + 1,
                    LiteralValue = characterToken.LiteralValue
                }
                : token).ToList();

        var lexer = new Lexer(pascalProgram);
        var actualTokens = lexer.Tokenize();
        for (int i = 0; i < expectedTokens.Count; i++)
        {
            _testOutputHelper.WriteLine($"Expect: {expectedTokens[i]}");
            _testOutputHelper.WriteLine($"Actual: {actualTokens[i]}");
            _testOutputHelper.WriteLine("----");
            Assert.Equal(expectedTokens[i], actualTokens[i]);
        }

        Assert.Equal(expectedTokens, actualTokens);
    }

    [Fact]
    public void TestLexicalAnalysisFirst()
    {
        string pascalProgram = """
        program HelloWorld;
        var
        message: string;
        begin
        message := 'hello, world!';
        writeln(message);
        end.
        """;

        var stringLiterals = new List<(string, SemanticTokenType, int)>
        {
            ("program", SemanticTokenType.Keyword, 0),
            ("HelloWorld", SemanticTokenType.Identifier, 0),
            (";", SemanticTokenType.Delimiter, 0),
            ("var", SemanticTokenType.Keyword, 0),
            ("message", SemanticTokenType.Identifier, 0),
            (":", SemanticTokenType.Delimiter, 0),
            ("string", SemanticTokenType.Identifier, 0),
            (";", SemanticTokenType.Delimiter, 1),
            ("begin", SemanticTokenType.Keyword, 0),
            ("message", SemanticTokenType.Identifier, 1),
            (":=", SemanticTokenType.Operator, 0),
            ("hello, world!", SemanticTokenType.Character, 0),
            (";", SemanticTokenType.Delimiter, 2),
            ("writeln", SemanticTokenType.Identifier, 0),
            ("(", SemanticTokenType.Delimiter, 0),
            ("message", SemanticTokenType.Identifier, 2),
            (")", SemanticTokenType.Delimiter, 0),
            (";", SemanticTokenType.Delimiter, 3),
            ("end", SemanticTokenType.Keyword, 0),
            (".", SemanticTokenType.Delimiter, 0)
        };
        TestLexicalAnalysis(pascalProgram, stringLiterals);
    }

    [Fact]
    public void TestLexicalAnalysisSecond()
    {
        string pascalProgram = """
        program main;
        var
          ab: integer;
        begin
          ab := 3;
          write(ab);
        end.
        """;

        var stringLiterals = new List<(string, SemanticTokenType, int)>
        {
            ("program", SemanticTokenType.Keyword, 0),
            ("main", SemanticTokenType.Identifier, 0),
            (";", SemanticTokenType.Delimiter, 0),
            ("var", SemanticTokenType.Keyword, 0),
            ("ab", SemanticTokenType.Identifier, 0),
            (":", SemanticTokenType.Delimiter, 0),
            ("integer", SemanticTokenType.Keyword, 0),
            (";", SemanticTokenType.Delimiter, 1),
            ("begin", SemanticTokenType.Keyword, 0),
            ("ab", SemanticTokenType.Identifier, 1),
            (":=", SemanticTokenType.Operator, 0),
            ("3", SemanticTokenType.Number, 0),
            (";", SemanticTokenType.Delimiter, 2),
            ("write", SemanticTokenType.Identifier, 0),
            ("(", SemanticTokenType.Delimiter, 0),
            ("ab", SemanticTokenType.Identifier, 2),
            (")", SemanticTokenType.Delimiter, 0),
            (";", SemanticTokenType.Delimiter, 3),
            ("end", SemanticTokenType.Keyword, 0),
            (".", SemanticTokenType.Delimiter, 0)
        };
        TestLexicalAnalysis(pascalProgram, stringLiterals);
    }

    //带注释的测试
    [Fact]
    public void TestLexicalAnalysisThird()
    {
        string pascalProgram = """
                                {test}
                                program main;
                                var
                                  ab, ba: integer;
                                begin
                                  ab := 3;
                                  ba := 5;
                                  ab := 5;
                                  write(ab + ba);
                                end.
                                """;

        var stringLiterals = new List<(string, SemanticTokenType, int)>
        {
            ("program", SemanticTokenType.Keyword, 0),
            ("main", SemanticTokenType.Identifier, 0),
            (";", SemanticTokenType.Delimiter, 0),
            ("var", SemanticTokenType.Keyword, 0),
            ("ab", SemanticTokenType.Identifier, 0),
            (",", SemanticTokenType.Delimiter, 0),
            ("ba", SemanticTokenType.Identifier, 0),
            (":", SemanticTokenType.Delimiter, 0),
            ("integer", SemanticTokenType.Keyword, 0),
            (";", SemanticTokenType.Delimiter, 1),
            ("begin", SemanticTokenType.Keyword, 0),
            ("ab", SemanticTokenType.Identifier, 1),
            (":=", SemanticTokenType.Operator, 0),
            ("3", SemanticTokenType.Number, 0),
            (";", SemanticTokenType.Delimiter, 2),
            ("ba", SemanticTokenType.Identifier, 1),
            (":=", SemanticTokenType.Operator, 1),
            ("5", SemanticTokenType.Number, 0),
            (";", SemanticTokenType.Delimiter, 3),
            ("ab", SemanticTokenType.Identifier, 2),
            (":=", SemanticTokenType.Operator, 2),
            ("5", SemanticTokenType.Number, 1),
            (";", SemanticTokenType.Delimiter, 4),
            ("write", SemanticTokenType.Identifier, 0),
            ("(", SemanticTokenType.Delimiter, 0),
            ("ab", SemanticTokenType.Identifier, 3),
            ("+", SemanticTokenType.Operator, 0),
            ("ba", SemanticTokenType.Identifier, 2),
            (")", SemanticTokenType.Delimiter, 0),
            (";", SemanticTokenType.Delimiter, 5),
            ("end", SemanticTokenType.Keyword, 0),
            (".", SemanticTokenType.Delimiter, 0)
        };
        TestLexicalAnalysis(pascalProgram, stringLiterals);
    }

    [Fact]
    public void UnclosedCommentFirst()
    {
        string pascalProgram = """
                                (* This is an example of an unclosed comment
                                program CommentError;
                                var
                                    x: integer;
                                begin
                                    x := 42;
                                end.
                                """;
        var lexer = new Lexer(pascalProgram);
        var ex = Assert.Throws<LexemeException>(() => lexer.Tokenize());
        //打印exception信息
        _testOutputHelper.WriteLine(ex.ToString());
        Assert.Equal(LexemeErrorType.UnclosedComment, ex.ErrorType);
        Assert.Equal((uint)7, ex.Line);
        Assert.Equal((uint)5, ex.CharPosition);
    }

    [Fact]
    public void UnclosedCommentSecond()
    {
        string pascalProgram = """
                               {
                                   This is a block comment that does not close.

                               program CommentNotClosed;
                               """;
        var lexer = new Lexer(pascalProgram);
        var ex = Assert.Throws<LexemeException>(() => lexer.Tokenize());
_testOutputHelper.WriteLine(ex.ToString());
        Assert.Equal(LexemeErrorType.UnclosedComment, ex.ErrorType);
        Assert.Equal((uint)4, ex.Line);
        Assert.Equal((uint)26, ex.CharPosition);
    }
}
