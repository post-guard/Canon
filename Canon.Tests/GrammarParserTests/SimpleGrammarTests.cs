using Canon.Core.Enums;
using Canon.Core.GrammarParser;

namespace Canon.Tests.GrammarParserTests;

public class SimpleGrammarTests
{
    /// <summary>
    /// 用于测试的简单语法
    /// S -> E
    /// E -> E+T | E-T | T
    /// T -> T*F | T/F | F
    /// F -> (E) | n
    /// 为了方便测试指定
    /// E ProgramStruct
    /// T ProgramBody
    /// F StatementList
    /// n Identifier
    /// </summary>
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
                    new NonTerminator(NonTerminatorType.ProgramStruct), new Terminator(OperatorType.Plus),
                    new NonTerminator(NonTerminatorType.ProgramBody)
                ],
                [
                    new NonTerminator(NonTerminatorType.ProgramStruct), new Terminator(OperatorType.Minus),
                    new NonTerminator(NonTerminatorType.ProgramBody)
                ],
                [new NonTerminator(NonTerminatorType.ProgramBody)]
            ]
        },
        {
            new NonTerminator(NonTerminatorType.ProgramBody), [
                [
                    new NonTerminator(NonTerminatorType.ProgramBody), new Terminator(OperatorType.Multiply),
                    new NonTerminator(NonTerminatorType.StatementList)
                ],
                [
                    new NonTerminator(NonTerminatorType.ProgramBody), new Terminator(OperatorType.Divide),
                    new NonTerminator(NonTerminatorType.StatementList)
                ],
                [new NonTerminator(NonTerminatorType.StatementList)]
            ]
        },
        {
            new NonTerminator(NonTerminatorType.StatementList), [
                [
                    new Terminator(DelimiterType.LeftParenthesis), new NonTerminator(NonTerminatorType.ProgramStruct),
                    new Terminator(DelimiterType.RightParenthesis)
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
            pair.Key == new NonTerminator(NonTerminatorType.StartNonTerminator));
        Assert.Contains(builder.FirstSet, pair =>
            pair.Key == new NonTerminator(NonTerminatorType.ProgramStruct));
        Assert.Contains(builder.FirstSet, pair =>
            pair.Key == new NonTerminator(NonTerminatorType.ProgramBody));
        Assert.Contains(builder.FirstSet, pair =>
            pair.Key == new NonTerminator(NonTerminatorType.StatementList));

        foreach (HashSet<Terminator> terminators in builder.FirstSet.Values)
        {
            Assert.Contains(Terminator.IdentifierTerminator, terminators);
            Assert.Contains(new Terminator(DelimiterType.LeftParenthesis), terminators);
        }
    }

    [Fact]
    public void StatsTest()
    {
        GrammarBuilder builder = new()
        {
            Generators = s_simpleGrammar, Begin = new NonTerminator(NonTerminatorType.StartNonTerminator)
        };

        Grammar grammar = builder.Build();

        Assert.Equal(30, builder.Automation.Count);

        // 来自Ichirinko不辞辛劳的手算
        Assert.Contains(new NonTerminator(NonTerminatorType.ProgramStruct), grammar.BeginState.Transformer.Keys);
        Assert.Contains(new NonTerminator(NonTerminatorType.ProgramBody), grammar.BeginState.Transformer.Keys);
        Assert.Contains(new NonTerminator(NonTerminatorType.StatementList),
            grammar.BeginState.Transformer.Keys);
        Assert.Contains(new Terminator(DelimiterType.LeftParenthesis), grammar.BeginState.Transformer.Keys);
        Assert.Contains(Terminator.IdentifierTerminator, grammar.BeginState.Transformer.Keys);
    }
}
