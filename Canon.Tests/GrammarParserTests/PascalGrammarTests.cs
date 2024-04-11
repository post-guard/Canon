using Canon.Core.Abstractions;
using Canon.Core.Enums;
using Canon.Core.GrammarParser;
using Canon.Core.LexicalParser;
using Canon.Core.SyntaxNodes;
using Canon.Tests.GeneratedParserTests;

namespace Canon.Tests.GrammarParserTests;

public class PascalGrammarTests
{
    private readonly GrammarParserBase _parser = GeneratedGrammarParser.Instance;

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

        ProgramStruct root = _parser.Analyse(tokens).Convert<ProgramStruct>();
        Assert.Equal("DoNothing", root.Head.ProgramName.LiteralValue);
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

        ProgramStruct root = _parser.Analyse(tokens).Convert<ProgramStruct>();
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

        ProgramStruct root = _parser.Analyse(tokens).Convert<ProgramStruct>();
        Assert.Equal("exFunction", root.Head.ProgramName.LiteralValue);
    }

    private static GrammarParserBase GenerateGrammarParser()
    {
        GrammarBuilder builder = new()
        {
            Generators = PascalGrammar.Grammar, Begin = new NonTerminator(NonTerminatorType.StartNonTerminator)
        };

        Grammar grammar = builder.Build();
        return grammar.ToGrammarParser();
    }
}
