using Canon.Core.Enums;
using Canon.Core.GrammarParser;
using Canon.Core.LexicalParser;
using Xunit.Abstractions;

namespace Canon.Tests.GrammarParserTests;

public class SimpleGrammarWithEmptyTests(ITestOutputHelper testOutputHelper)
{
    /// <summary>
    /// 带有空产生式的简单语法(课后题4.18)
    /// S -> A
    /// A -> BA | ε
    /// B -> aB | a
    /// 为了方便测试指定
    /// A ProgramStruct
    /// B ProgramBody
    /// a Identifier
    /// </summary>
    // private readonly ITestOutputHelper _testOutputHelper;

    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

    private static readonly Dictionary<NonTerminator, List<List<TerminatorBase>>> s_simpleGrammar = new()
    {
        {
            new NonTerminator(NonTerminatorType.StartNonTerminator), [
                [new NonTerminator(NonTerminatorType.ProgramStruct)]
            ]
        },
        {
            new NonTerminator(NonTerminatorType.ProgramStruct), [
                [
                    new NonTerminator(NonTerminatorType.ProgramBody),
                    new NonTerminator(NonTerminatorType.ProgramStruct)
                ],
                [Terminator.EmptyTerminator]
            ]
        },
        {
            new NonTerminator(NonTerminatorType.ProgramBody), [
                [
                    Terminator.IdentifierTerminator,
                    new NonTerminator(NonTerminatorType.ProgramBody)
                ],
                [Terminator.IdentifierTerminator]
            ]
        }
    };

    [Fact]
    public void FirstSetTest()
    {
        GrammarBuilder builder = new()
        {
            Generators = s_simpleGrammar, Begin = new NonTerminator(NonTerminatorType.StartNonTerminator)
        };

        builder.Build();

        Assert.Contains(builder.FirstSet, pair =>
        {
            if (pair.Key == new NonTerminator(NonTerminatorType.StartNonTerminator))
            {
                Assert.Equal(2, pair.Value.Count);
                Assert.Contains(Terminator.IdentifierTerminator, pair.Value);
                Assert.Contains(Terminator.EmptyTerminator, pair.Value);
                return true;
            }

            return false;
        });

        Assert.Contains(builder.FirstSet, pair =>
        {
            if (pair.Key == new NonTerminator(NonTerminatorType.ProgramStruct))
            {
                Assert.Equal(2, pair.Value.Count);
                Assert.Contains(Terminator.IdentifierTerminator, pair.Value);
                Assert.Contains(Terminator.EmptyTerminator, pair.Value);
                return true;
            }

            return true;
        });
        Assert.Contains(builder.FirstSet, pair =>
        {
            if (pair.Key == new NonTerminator(NonTerminatorType.ProgramBody))
            {
                Assert.Single(pair.Value);
                Assert.Contains(Terminator.IdentifierTerminator, pair.Value);
                return true;
            }

            return false;
        });
    }

    [Fact]
    public void StatesTest()
    {
        GrammarBuilder builder = new()
        {
            Generators = s_simpleGrammar, Begin = new NonTerminator(NonTerminatorType.StartNonTerminator)
        };

        Grammar grammar = builder.Build();

        Assert.Equal(6, builder.Automation.Count);

        Assert.Contains(new NonTerminator(NonTerminatorType.ProgramStruct), grammar.BeginState.Transformer.Keys);
        Assert.Contains(new NonTerminator(NonTerminatorType.ProgramBody), grammar.BeginState.Transformer.Keys);
        Assert.Contains(Terminator.IdentifierTerminator, grammar.BeginState.Transformer.Keys);
        Assert.Equal(7, grammar.BeginState.Expressions.Count);
        _testOutputHelper.WriteLine("--- 0 ---");
        _testOutputHelper.WriteLine(grammar.BeginState.ToString());

        LrState state1 =
            grammar.BeginState.Transformer[new NonTerminator(NonTerminatorType.ProgramStruct)];
        Assert.Single(state1.Expressions);
        _testOutputHelper.WriteLine("--- 1 ---");
        _testOutputHelper.WriteLine(state1.ToString());

        LrState state2 =
            grammar.BeginState.Transformer[new NonTerminator(NonTerminatorType.ProgramBody)];
        Assert.Contains(new NonTerminator(NonTerminatorType.ProgramStruct), state2.Transformer.Keys);
        Assert.Contains(new NonTerminator(NonTerminatorType.ProgramBody), state2.Transformer.Keys);
        Assert.Contains(Terminator.IdentifierTerminator, state2.Transformer.Keys);
        Assert.Equal(7, state2.Expressions.Count);
        _testOutputHelper.WriteLine("--- 2 ---");
        _testOutputHelper.WriteLine(state2.ToString());

        LrState state3 =
            grammar.BeginState.Transformer[Terminator.IdentifierTerminator];
        Assert.Contains(new NonTerminator(NonTerminatorType.ProgramBody), state3.Transformer.Keys);
        Assert.Contains(Terminator.IdentifierTerminator, state3.Transformer.Keys);
        Assert.Equal(8, state3.Expressions.Count);
        _testOutputHelper.WriteLine("--- 3 ---");
        _testOutputHelper.WriteLine(state3.ToString());

        LrState state4 = state2.Transformer[new NonTerminator(NonTerminatorType.ProgramStruct)];
        Assert.Empty(state4.Transformer);
        Assert.Single(state4.Expressions);
        _testOutputHelper.WriteLine("--- 4 ---");
        _testOutputHelper.WriteLine(state4.ToString());

        LrState state5 = state3.Transformer[new NonTerminator(NonTerminatorType.ProgramBody)];
        Assert.Empty(state5.Transformer);
        Assert.Equal(2, state5.Expressions.Count);
        _testOutputHelper.WriteLine("--- 5 ---");
        _testOutputHelper.WriteLine(state5.ToString());
    }

    [Fact]
    public void AnalyseSingleSentenceTest()
    {
        GrammarBuilder builder = new()
        {
            Generators = s_simpleGrammar, Begin = new NonTerminator(NonTerminatorType.StartNonTerminator)
        };

        Grammar grammar = builder.Build();

        List<SemanticToken> tokens =
        [
            new IdentifierSemanticToken { LinePos = 0, CharacterPos = 0, LiteralValue = "a" },
            new IdentifierSemanticToken { LinePos = 0, CharacterPos = 0, LiteralValue = "a" },
            SemanticToken.End
        ];

        SyntaxNode root = grammar.Analyse(tokens);

        Assert.False(root.IsTerminated);
        Assert.Equal(NonTerminatorType.ProgramStruct, root.GetNonTerminatorType());
        Assert.Equal(7, root.Count());
    }
}
