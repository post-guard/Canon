using Canon.Core.Abstractions;
using Canon.Core.Enums;
using Canon.Core.GrammarParser;
using Canon.Core.LexicalParser;
using Canon.Core.SyntaxNodes;

namespace Canon.Tests.GrammarParserTests;

public class PascalGrammarTests
{
    private readonly GrammarBuilder _builder = new()
    {
        Generators = PascalGrammar.Grammar, Begin = new NonTerminator(NonTerminatorType.StartNonTerminator)
    };

    private readonly GrammarParserBase _parser;

    public PascalGrammarTests()
    {
        Grammar grammar = _builder.Build();
        _parser = grammar.ToGrammarParser();
    }

    [Fact]
    public void GrammarTest()
    {
        Assert.NotNull(_parser);
    }

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
}
