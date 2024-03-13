using Canon.Core.Abstractions;
using Canon.Core.Enums;
using Canon.Core.GrammarParser;

namespace Canon.Tests.GrammarParserTests;

public partial class PascalGrammarTests
{
    private readonly GrammarBuilder _builder = new()
    {
        Generators = s_pascalGrammar, Begin = new NonTerminator(NonTerminatorType.StartNonTerminator)
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
}
