using Canon.Core.Enums;
using Canon.Core.Exceptions;
using Canon.Core.LexicalParser;
using Canon.Tests.Utils;
using Canon.Core.Abstractions;
using Xunit.Abstractions;

namespace Canon.Tests.LexicalParserTests;

public class LexicalFileTests(ITestOutputHelper testOutputHelper)
{
    private readonly ILexer _lexer = new Lexer();

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

        IEnumerable<SemanticToken> tokens = _lexer.Tokenize(new StringSourceReader(pascalProgram));
        ValidateSemanticTokens(tokens, [
            SemanticTokenType.Keyword,
            SemanticTokenType.Identifier,
            SemanticTokenType.Delimiter,
            SemanticTokenType.Keyword,
            SemanticTokenType.Identifier,
            SemanticTokenType.Delimiter,
            SemanticTokenType.Identifier,
            SemanticTokenType.Delimiter,
            SemanticTokenType.Keyword,
            SemanticTokenType.Identifier,
            SemanticTokenType.Operator,
            SemanticTokenType.Character,
            SemanticTokenType.Delimiter,
            SemanticTokenType.Identifier,
            SemanticTokenType.Delimiter,
            SemanticTokenType.Identifier,
            SemanticTokenType.Delimiter,
            SemanticTokenType.Delimiter,
            SemanticTokenType.Keyword,
            SemanticTokenType.Delimiter
        ]);
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

        IEnumerable<SemanticToken> tokens = _lexer.Tokenize(new StringSourceReader(pascalProgram));

        ValidateSemanticTokens(tokens, [
            SemanticTokenType.Keyword,
            SemanticTokenType.Identifier,
            SemanticTokenType.Delimiter,
            SemanticTokenType.Keyword,
            SemanticTokenType.Identifier,
            SemanticTokenType.Delimiter,
            SemanticTokenType.Keyword,
            SemanticTokenType.Delimiter,
            SemanticTokenType.Keyword,
            SemanticTokenType.Identifier,
            SemanticTokenType.Operator,
            SemanticTokenType.Number,
            SemanticTokenType.Delimiter,
            SemanticTokenType.Identifier,
            SemanticTokenType.Delimiter,
            SemanticTokenType.Identifier,
            SemanticTokenType.Delimiter,
            SemanticTokenType.Delimiter,
            SemanticTokenType.Keyword,
            SemanticTokenType.Delimiter
        ]);
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

        IEnumerable<SemanticToken> tokens = _lexer.Tokenize(new StringSourceReader(pascalProgram));

        ValidateSemanticTokens(tokens, [
            SemanticTokenType.Keyword,
            SemanticTokenType.Identifier,
            SemanticTokenType.Delimiter,
            SemanticTokenType.Keyword,
            SemanticTokenType.Identifier,
            SemanticTokenType.Delimiter,
            SemanticTokenType.Identifier,
            SemanticTokenType.Delimiter,
            SemanticTokenType.Keyword,
            SemanticTokenType.Delimiter,
            SemanticTokenType.Keyword,
            SemanticTokenType.Identifier,
            SemanticTokenType.Operator,
            SemanticTokenType.Number,
            SemanticTokenType.Delimiter,
            SemanticTokenType.Identifier,
            SemanticTokenType.Operator,
            SemanticTokenType.Number,
            SemanticTokenType.Delimiter,
            SemanticTokenType.Identifier,
            SemanticTokenType.Operator,
            SemanticTokenType.Number,
            SemanticTokenType.Delimiter,
            SemanticTokenType.Identifier,
            SemanticTokenType.Delimiter,
            SemanticTokenType.Identifier,
            SemanticTokenType.Operator,
            SemanticTokenType.Identifier,
            SemanticTokenType.Delimiter,
            SemanticTokenType.Delimiter,
            SemanticTokenType.Keyword,
            SemanticTokenType.Delimiter
        ]);
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
        var ex = Assert.Throws<LexemeException>(() => _lexer.Tokenize(new StringSourceReader(pascalProgram)).ToList());
        //打印exception信息
        testOutputHelper.WriteLine(ex.ToString());
        Assert.Equal(LexemeErrorType.UnclosedComment, ex.ErrorType);
        Assert.Equal((uint)7, ex.Line);
        Assert.Equal((uint)4, ex.CharPosition);
    }

    [Fact]
    public void UnclosedCommentSecond()
    {
        string pascalProgram = """
                               {
                                   This is a block comment that does not close.

                               program CommentNotClosed;
                               """;
        var ex = Assert.Throws<LexemeException>(() => _lexer.Tokenize(new StringSourceReader(pascalProgram)).ToList());
        testOutputHelper.WriteLine(ex.ToString());
        Assert.Equal(LexemeErrorType.UnclosedComment, ex.ErrorType);
        Assert.Equal((uint)4, ex.Line);
        Assert.Equal((uint)25, ex.CharPosition);
    }

    [Fact]
    public void ClosedCommentFirst()
    {
        string pascalProgram = """
                               program exFunction;
                               var
                               a, b, ret : integer;

                               begin
                                    a := 100;
                                    b := 200;
                                    (* calling a function to get max value
                                    *)
                                    ret := a - b;



                               end.
                               """;
        IEnumerable<SemanticToken> tokensEnumerable = _lexer.Tokenize(new StringSourceReader(pascalProgram));
        List<SemanticToken> tokens = tokensEnumerable.ToList();
        Assert.NotNull(tokens);
    }

    [Fact]
    public void ClosedCommentSecond()
    {
        string pascalProgram = """
                               program exFunction;
                               var
                               a, b, ret : integer;

                               begin
                                    a := 100;
                                    b := 200;
                                    (* calling a function to get max valued *)
                                    ret := a - b;



                               end.
                               """;
        IEnumerable<SemanticToken> tokensEnumerable = _lexer.Tokenize(new StringSourceReader(pascalProgram));
        List<SemanticToken> tokens = tokensEnumerable.ToList();
        Assert.NotNull(tokens);
    }


    [Fact]
    public void ClosedCommentThird()
    {
        string pascalProgram = """
                               {
                                   This is a block comment that does closed.
                                 }
                               program CommentClosed;
                               """;
        IEnumerable<SemanticToken> tokensEnumerable = _lexer.Tokenize(new StringSourceReader(pascalProgram));
        List<SemanticToken> tokens = tokensEnumerable.ToList();
        Assert.NotNull(tokens);
    }

    [Fact]
    public void ClosedCommentFourth()
    {
        string pascalProgram = """
                               {}
                               program CommentClosed;
                               """;
        IEnumerable<SemanticToken> tokensEnumerable = _lexer.Tokenize(new StringSourceReader(pascalProgram));
        List<SemanticToken> tokens = tokensEnumerable.ToList();
        Assert.NotNull(tokens);
    }

    [Fact]
    public void ClosedCommentFifth()
    {
        string pascalProgram = """
                               {
                               }
                               program CommentClosed;
                               """;
        IEnumerable<SemanticToken> tokensEnumerable = _lexer.Tokenize(new StringSourceReader(pascalProgram));
        List<SemanticToken> tokens = tokensEnumerable.ToList();
        Assert.NotNull(tokens);
    }

    [Fact]
    public void ClosedCommentSixth()
    {
        string pascalProgram = """
                               (**)
                               """;
        IEnumerable<SemanticToken> tokensEnumerable = _lexer.Tokenize(new StringSourceReader(pascalProgram));
        List<SemanticToken> tokens = tokensEnumerable.ToList();
        Assert.NotNull(tokens);
    }

    private static void ValidateSemanticTokens(IEnumerable<SemanticToken> actualTokens,
        IEnumerable<SemanticTokenType> expectedTypes)
    {
        List<SemanticTokenType> types = [..expectedTypes, SemanticTokenType.End];
        List<SemanticToken> tokens = actualTokens.ToList();

        Assert.Equal(types.Count, tokens.Count);
        foreach ((SemanticTokenType type, SemanticToken token) in types.Zip(tokens))
        {
            Assert.Equal(type, token.TokenType);
        }
    }
}
