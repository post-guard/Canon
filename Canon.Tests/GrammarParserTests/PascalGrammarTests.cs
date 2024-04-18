using Canon.Core.Abstractions;
using Canon.Core.Enums;
using Canon.Core.GrammarParser;
using Canon.Core.LexicalParser;
using Canon.Core.SyntaxNodes;
using Canon.Tests.GeneratedParserTests;
using Canon.Tests.Utils;

namespace Canon.Tests.GrammarParserTests;

public class PascalGrammarTests
{
    private readonly IGrammarParser _parser = GeneratedGrammarParser.Instance;
    private readonly ILexer _lexer = new Lexer();

    [Fact]
    public void DoNothingTest()
    {
        const string program = """
                         program DoNothing;
                         begin
                         end.
                         """;

        IEnumerable<SemanticToken> tokens = _lexer.Tokenize(new StringSourceReader(program));

        ProgramStruct root = _parser.Analyse(tokens);
        Assert.Equal("DoNothing", root.Head.ProgramName.LiteralValue);
        Assert.Equal(15, root.Count());
    }

    [Fact]
    public void AddTest()
    {
        const string program = """
                               program Add;
                               var a : Integer;
                               begin
                               a := 1 + 1
                               end.
                               """;
        IEnumerable<SemanticToken> tokens = _lexer.Tokenize(new StringSourceReader(program));

        ProgramStruct root = _parser.Analyse(tokens);
        Assert.Equal("Add", root.Head.ProgramName.LiteralValue);
    }

    [Fact]
    public void WriteLnTest()
    {
        const string program = """
                               program exFunction;
                               const str = 'result is : ';
                               var a, b : Integer;
                               begin
                               writeln( str, ret );
                               end.
                               """;
        IEnumerable<SemanticToken> tokens = _lexer.Tokenize(new StringSourceReader(program));

        ProgramStruct root = _parser.Analyse(tokens);
        Assert.Equal("exFunction", root.Head.ProgramName.LiteralValue);
    }

    [Fact]
    public void SubprogramTest()
    {
        const string program = """
                               program main;
                               procedure test;
                               begin
                               end;
                               begin
                               end.
                               """;
        IEnumerable<SemanticToken> tokens = _lexer.Tokenize(new StringSourceReader(program));

        ProgramStruct root = _parser.Analyse(tokens);
        Assert.Equal("main", root.Head.ProgramName.LiteralValue);
    }
}
