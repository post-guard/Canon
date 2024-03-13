using Canon.Core.Abstractions;
using Canon.Core.Enums;
using Canon.Core.GrammarParser;
using Canon.Core.LexicalParser;

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
    public void StatesTest()
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

    [Fact]
    public void AnalyseSingleSentenceTest()
    {
        GrammarBuilder builder = new()
        {
            Generators = s_simpleGrammar, Begin = new NonTerminator(NonTerminatorType.StartNonTerminator)
        };

        Grammar grammar = builder.Build();
        GrammarParserBase parser = grammar.ToGrammarParser();
        // n + n
        List<SemanticToken> tokens =
        [
            new IdentifierSemanticToken { LinePos = 0, CharacterPos = 0, LiteralValue = "n" },
            new OperatorSemanticToken
            {
                LinePos = 0, CharacterPos = 0, LiteralValue = "+", OperatorType = OperatorType.Plus
            },
            new IdentifierSemanticToken { LinePos = 0, CharacterPos = 0, LiteralValue = "n" },
            SemanticToken.End
        ];

        // 分析树为
        //        E
        //        |
        //       /\
        //     / | \
        //    E  +  T
        //    |     |
        //    T     F
        //    |     |
        //   F     n
        //   |
        //   n
        SyntaxNode root = parser.Analyse(tokens);
        Assert.Equal(NonTerminatorType.ProgramStruct, root.GetNonTerminatorType());
        Assert.Equal(3, root.Children.Count);
        Assert.Contains(root.Children, node =>
        {
            if (node.IsTerminated && node.GetSemanticToken().TokenType == SemanticTokenType.Operator)
            {
                OperatorSemanticToken token = (OperatorSemanticToken)node.GetSemanticToken();

                return token.OperatorType == OperatorType.Plus;
            }

            return false;
        });
        Assert.Equal(9, root.Count());
    }

    [Fact]
    public void AnalyseComplexSentenceTest()
    {
        GrammarBuilder builder = new()
        {
            Generators = s_simpleGrammar, Begin = new NonTerminator(NonTerminatorType.StartNonTerminator)
        };

        Grammar grammar = builder.Build();
        GrammarParserBase parser = grammar.ToGrammarParser();

        // (n + n) * n
        List<SemanticToken> tokens =
        [
            new DelimiterSemanticToken
            {
                LinePos = 0, CharacterPos = 0, LiteralValue = "(", DelimiterType = DelimiterType.LeftParenthesis
            },
            new IdentifierSemanticToken { LinePos = 0, CharacterPos = 0, LiteralValue = "n" },
            new OperatorSemanticToken
            {
                LinePos = 0, CharacterPos = 0, LiteralValue = "+", OperatorType = OperatorType.Plus
            },
            new IdentifierSemanticToken { LinePos = 0, CharacterPos = 0, LiteralValue = "n" },
            new DelimiterSemanticToken
            {
                LinePos = 0, CharacterPos = 0, LiteralValue = ")", DelimiterType = DelimiterType.RightParenthesis
            },
            new OperatorSemanticToken
            {
                LinePos = 0, CharacterPos = 0, LiteralValue = "*", OperatorType = OperatorType.Multiply
            },
            new IdentifierSemanticToken { LinePos = 0, CharacterPos = 0, LiteralValue = "n" },
            SemanticToken.End
        ];


        SyntaxNode root = parser.Analyse(tokens);
        Assert.Equal(18, root.Count());
        Assert.False(root.IsTerminated);
        Assert.Equal(NonTerminatorType.ProgramStruct, root.GetNonTerminatorType());
        Assert.Single(root.Children);
    }
}
