using Canon.Core.Abstractions;
using Canon.Core.Enums;
using Canon.Core.GrammarParser;

namespace Canon.Tests.GeneratedParserTests;

public class GenerateParserTests
{
    private readonly GrammarBuilder _builder = new()
    {
        Generators = PascalGrammar.Grammar, Begin = new NonTerminator(NonTerminatorType.StartNonTerminator)
    };

    private readonly IGrammarParser _parser;

    public GenerateParserTests()
    {
        Grammar grammar = _builder.Build();
        _parser = grammar.ToGrammarParser();
    }

    [Fact]
    public void ConsistencyTests()
    {
        GeneratedGrammarParser generatedGrammarParser = GeneratedGrammarParser.Instance;

        ITransformer originTransformer = _parser.BeginTransformer;
        ITransformer generatedTransformer = generatedGrammarParser.BeginTransformer;

        Queue<(ITransformer, ITransformer)> transformerQueue = [];
        transformerQueue.Enqueue((originTransformer, generatedTransformer));
        HashSet<string> visited = [];

        while (transformerQueue.Count != 0)
        {
            (originTransformer, generatedTransformer) = transformerQueue.Dequeue();
            if (visited.Contains(generatedTransformer.Name))
            {
                continue;
            }

            visited.Add(generatedTransformer.Name);

            foreach (KeyValuePair<TerminatorBase,ITransformer> pair in originTransformer.ShiftTable)
            {
                Assert.True(generatedTransformer.ShiftTable.TryGetValue(pair.Key, out ITransformer? nextTransformer));
                Assert.NotNull(nextTransformer);

                transformerQueue.Enqueue((pair.Value, nextTransformer));
            }

            foreach (KeyValuePair<Terminator,ReduceInformation> pair in originTransformer.ReduceTable)
            {
                Assert.True(generatedTransformer.ReduceTable.TryGetValue(pair.Key, out ReduceInformation? information));
                Assert.NotNull(information);

                Assert.Equal(pair.Value.Length, information.Length);
                Assert.Equal(pair.Value.Left, information.Left);
            }
        }
    }

    [Fact]
    public void SubprogramDeclarationsFirstSetTest()
    {
        Assert.True(_builder.FirstSet.TryGetValue(
            new NonTerminator(NonTerminatorType.SubprogramDeclarations), out HashSet<Terminator>? firstSet));
        Assert.NotNull(firstSet);
        Assert.Contains(Terminator.EmptyTerminator, firstSet);
        Assert.Contains(new Terminator(KeywordType.Procedure), firstSet);
        Assert.Contains(new Terminator(KeywordType.Function), firstSet);
    }
}
