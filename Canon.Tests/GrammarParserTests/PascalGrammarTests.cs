using Canon.Core.Abstractions;
using Canon.Core.Enums;
using Canon.Core.GrammarParser;
using Canon.Core.LexicalParser;
using Canon.Core.SyntaxNodes;
using Canon.Tests.GeneratedParserTests;

namespace Canon.Tests.GrammarParserTests;

public class PascalGrammarTests
{
    private readonly IGrammarParser _parser = GeneratedGrammarParser.Instance;

    [Fact]
    public void DoNothingTest()
    {
        const string program = """
                         program DoNothing;
                         begin
                         end.
                         """;

        Lexer lexer = new(program);
        List<SemanticToken> tokens = lexer.Tokenize();
        tokens.Add(SemanticToken.End);

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

        Lexer lexer = new(program);
        List<SemanticToken> tokens = lexer.Tokenize();
        tokens.Add(SemanticToken.End);

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

        Lexer lexer = new(program);
        List<SemanticToken> tokens = lexer.Tokenize();
        tokens.Add(SemanticToken.End);

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

        Lexer lexer = new(program);
        List<SemanticToken> tokens = lexer.Tokenize();
        tokens.Add(SemanticToken.End);

        ProgramStruct root = _parser.Analyse(tokens);
        Assert.Equal("main", root.Head.ProgramName.LiteralValue);
    }
}
