using Canon.Core.Abstractions;
using Canon.Core.Enums;
using Canon.Core.GrammarParser;
using Canon.Core.LexicalParser;

namespace Canon.Tests.GrammarParserTests;

public class PascalGrammarConstPart
{
    private static readonly Dictionary<NonTerminator, List<List<TerminatorBase>>> s_pascalGrammar = new()
    {
        {
            // ProgramStart -> ProgramBody
            new NonTerminator(NonTerminatorType.StartNonTerminator), [
                [new NonTerminator(NonTerminatorType.ProgramBody)]
            ]
        },
        {
            // ProgramBody -> ConstDeclarations
            new NonTerminator(NonTerminatorType.ProgramBody), [
                [
                    new NonTerminator(NonTerminatorType.ConstDeclarations),
                    // new NonTerminator(NonTerminatorType.VarDeclarations),
                    // new NonTerminator(NonTerminatorType.SubprogramDeclarations),
                    // new NonTerminator(NonTerminatorType.CompoundStatement)
                ]
            ]
        },
        {
            // ConstDeclarations -> ε | const ConstDeclaration ;
            new NonTerminator(NonTerminatorType.ConstDeclarations), [
                [
                    Terminator.EmptyTerminator,
                ],
                [
                    new Terminator(KeywordType.Const),
                    new NonTerminator(NonTerminatorType.ConstDeclaration),
                    new Terminator(DelimiterType.Semicolon)
                ]
            ]
        },
        {
            // ConstDeclaration -> id = ConstValue | ConstDeclaration ; id = ConstValue
            new NonTerminator(NonTerminatorType.ConstDeclaration), [
                [
                    Terminator.IdentifierTerminator,
                    new Terminator(OperatorType.Equal),
                    new NonTerminator(NonTerminatorType.ConstValue)
                ],
                [
                    new NonTerminator(NonTerminatorType.ConstDeclaration),
                    new Terminator(DelimiterType.Semicolon),
                    Terminator.IdentifierTerminator,
                    new Terminator(OperatorType.Equal),
                    new NonTerminator(NonTerminatorType.ConstValue)
                ]
            ]
        },
        {
            // ConstValue -> +num | -num | num | 'letter'
            new NonTerminator(NonTerminatorType.ConstValue), [
                [
                    new Terminator(OperatorType.Plus), Terminator.NumberTerminator
                ],
                [
                    new Terminator(OperatorType.Minus), Terminator.NumberTerminator,
                ],
                [
                    Terminator.NumberTerminator,
                ],
                [
                    new Terminator(DelimiterType.SingleQuotation),
                    Terminator.CharacterTerminator,
                    new Terminator(DelimiterType.SingleQuotation),
                ]
            ]
        }
    };

    [Fact]
    public void AstTestFirst()
    {
        GrammarBuilder builder = new()
        {
            Generators = s_pascalGrammar, Begin = new NonTerminator(NonTerminatorType.StartNonTerminator)
        };

        GrammarParserBase grammar = builder.Build().ToGrammarParser();

        // const a = +5;
        List<SemanticToken> tokens =
        [
            new KeywordSemanticToken
            {
                CharacterPos = 0, KeywordType = KeywordType.Const, LinePos = 0, LiteralValue = "const"
            },
            new IdentifierSemanticToken { LinePos = 0, CharacterPos = 0, LiteralValue = "a" },
            new OperatorSemanticToken
            {
                LinePos = 0, CharacterPos = 0, LiteralValue = "=", OperatorType = OperatorType.Equal
            },
            new OperatorSemanticToken
            {
                LinePos = 0, CharacterPos = 0, LiteralValue = "+", OperatorType = OperatorType.Plus
            },
            new NumberSemanticToken
            {
                CharacterPos = 0, LinePos = 0, LiteralValue = "5", NumberType = NumberType.Integer
            },
            new DelimiterSemanticToken
            {
                CharacterPos = 0, DelimiterType = DelimiterType.Semicolon, LinePos = 0, LiteralValue = ";"
            },
            SemanticToken.End
        ];


        // ProgramBody
        SyntaxNode root = grammar.Analyse(tokens);
        Assert.Equal(NonTerminatorType.ProgramBody, root.GetNonTerminatorType());
        Assert.Single(root.Children);
        Assert.Equal(10, root.Count());

        // ConstDeclarations
        root = root.Children[0];
        Assert.Equal(NonTerminatorType.ConstDeclarations, root.GetNonTerminatorType());
        Assert.Equal(3, root.Children.Count);

        Assert.Contains(root.Children, node =>
        {
            if (node.IsTerminated && node.GetSemanticToken().TokenType == SemanticTokenType.Keyword)
            {
                KeywordSemanticToken token = (KeywordSemanticToken)node.GetSemanticToken();

                return token.KeywordType == KeywordType.Const;
            }

            if (node.IsTerminated && node.GetSemanticToken().TokenType == SemanticTokenType.Delimiter)
            {
                DelimiterSemanticToken token = (DelimiterSemanticToken)node.GetSemanticToken();

                return token.DelimiterType == DelimiterType.Semicolon;
            }

            if (!node.IsTerminated && node.GetNonTerminatorType() == NonTerminatorType.ConstDeclaration)
            {
                return true;
            }

            return false;
        });

        // ConstDeclaration
        root = root.Children.Where(child =>
            !child.IsTerminated && child.GetNonTerminatorType() == NonTerminatorType.ConstDeclaration
        ).ElementAt(0);

        Assert.Equal(NonTerminatorType.ConstDeclaration, root.GetNonTerminatorType());
        Assert.Equal(3, root.Children.Count);
        Assert.Contains(root.Children, node =>
        {
            if (node.IsTerminated && node.GetSemanticToken().TokenType == SemanticTokenType.Identifier)
            {
                return true;
            }

            if (!node.IsTerminated && node.GetNonTerminatorType() == NonTerminatorType.ConstValue)
            {
                return true;
            }

            if (node.IsTerminated && node.GetSemanticToken().TokenType == SemanticTokenType.Operator)
            {
                OperatorSemanticToken token = (OperatorSemanticToken)node.GetSemanticToken();

                return token.OperatorType == OperatorType.Equal;
            }

            return false;
        });

        // ConstValue
        root = root.Children.Where(child =>
            !child.IsTerminated && child.GetNonTerminatorType() == NonTerminatorType.ConstValue
        ).ElementAt(0);
        Assert.Equal(NonTerminatorType.ConstValue, root.GetNonTerminatorType());
        Assert.Equal(2, root.Children.Count);
        Assert.Contains(root.Children, node =>
        {
            if (node.IsTerminated && node.GetSemanticToken().TokenType == SemanticTokenType.Operator)
            {
                OperatorSemanticToken token = (OperatorSemanticToken)node.GetSemanticToken();

                return token.OperatorType == OperatorType.Plus;
            }

            if (node.IsTerminated && node.GetSemanticToken().TokenType == SemanticTokenType.Number)
            {
                return true;
            }

            return false;
        });
    }

    [Fact]
    public void AstTestSecond()
    {
        GrammarBuilder builder = new()
        {
            Generators = s_pascalGrammar, Begin = new NonTerminator(NonTerminatorType.StartNonTerminator)
        };

        GrammarParserBase grammar = builder.Build().ToGrammarParser();

        // const a = 5; McCafe = 'Under Pressure (Queen & D. Bowie)' ;
        List<SemanticToken> tokens =
        [
            new KeywordSemanticToken
            {
                CharacterPos = 0, KeywordType = KeywordType.Const, LinePos = 0, LiteralValue = "const"
            },
            new IdentifierSemanticToken { LinePos = 0, CharacterPos = 0, LiteralValue = "a" },
            new OperatorSemanticToken
            {
                LinePos = 0, CharacterPos = 0, LiteralValue = "=", OperatorType = OperatorType.Equal
            },
            new NumberSemanticToken
            {
                CharacterPos = 0, LinePos = 0, LiteralValue = "5", NumberType = NumberType.Integer
            },
            new DelimiterSemanticToken
            {
                CharacterPos = 0, DelimiterType = DelimiterType.Semicolon, LinePos = 0, LiteralValue = ";"
            },
            new IdentifierSemanticToken { LinePos = 0, CharacterPos = 0, LiteralValue = "McCafe" },
            new OperatorSemanticToken
            {
                LinePos = 0, CharacterPos = 0, LiteralValue = "=", OperatorType = OperatorType.Equal
            },
            new DelimiterSemanticToken
            {
                CharacterPos = 0, DelimiterType = DelimiterType.SingleQuotation, LinePos = 0, LiteralValue = "'"
            },
            new CharacterSemanticToken
            {
                CharacterPos = 0, LinePos = 0, LiteralValue = "Under Pressure (Queen & D. Bowie)"
            },
            new DelimiterSemanticToken
            {
                CharacterPos = 0, DelimiterType = DelimiterType.SingleQuotation, LinePos = 0, LiteralValue = "'"
            },
            new DelimiterSemanticToken
            {
                CharacterPos = 0, DelimiterType = DelimiterType.Semicolon, LinePos = 0, LiteralValue = ";"
            },
            SemanticToken.End
        ];

        // 分析树见文档

        // ProgramBody
        SyntaxNode root = grammar.Analyse(tokens);
        Assert.Equal(NonTerminatorType.ProgramBody, root.GetNonTerminatorType());
        Assert.Single(root.Children);
        Assert.Equal(17, root.Count());

        // ConstDeclarations
        root = root.Children[0];
        Assert.Equal(NonTerminatorType.ConstDeclarations, root.GetNonTerminatorType());
        Assert.Equal(3, root.Children.Count);

        Assert.Contains(root.Children, node =>
        {
            if (node.IsTerminated && node.GetSemanticToken().TokenType == SemanticTokenType.Keyword)
            {
                KeywordSemanticToken token = (KeywordSemanticToken)node.GetSemanticToken();

                return token.KeywordType == KeywordType.Const;
            }

            if (node.IsTerminated && node.GetSemanticToken().TokenType == SemanticTokenType.Delimiter)
            {
                DelimiterSemanticToken token = (DelimiterSemanticToken)node.GetSemanticToken();

                return token.DelimiterType == DelimiterType.Semicolon;
            }

            if (!node.IsTerminated && node.GetNonTerminatorType() == NonTerminatorType.ConstDeclaration)
            {
                return true;
            }

            return false;
        });

        // ConstDeclaration layer3
        root = root.Children.Where(child =>
            !child.IsTerminated && child.GetNonTerminatorType() == NonTerminatorType.ConstDeclaration
        ).ElementAt(0);

        Assert.Equal(NonTerminatorType.ConstDeclaration, root.GetNonTerminatorType());
        Assert.Equal(5, root.Children.Count);
        Assert.Contains(root.Children, node =>
        {
            if (!node.IsTerminated && node.GetNonTerminatorType() == NonTerminatorType.ConstDeclaration)
            {
                return true;
            }

            if (node.IsTerminated && node.GetSemanticToken().TokenType == SemanticTokenType.Delimiter)
            {
                DelimiterSemanticToken token = (DelimiterSemanticToken)node.GetSemanticToken();

                return token.DelimiterType == DelimiterType.Semicolon;
            }

            if (node.IsTerminated && node.GetSemanticToken().TokenType == SemanticTokenType.Identifier)
            {
                return true;
            }

            if (!node.IsTerminated && node.GetNonTerminatorType() == NonTerminatorType.ConstValue)
            {
                return true;
            }

            if (node.IsTerminated && node.GetSemanticToken().TokenType == SemanticTokenType.Operator)
            {
                OperatorSemanticToken token = (OperatorSemanticToken)node.GetSemanticToken();

                return token.OperatorType == OperatorType.Equal;
            }

            return false;
        });

        // ConstDeclaration layer4
        SyntaxNode constDeclarationLayer4 = root.Children.Where(child =>
            !child.IsTerminated && child.GetNonTerminatorType() == NonTerminatorType.ConstDeclaration
        ).ElementAt(0);
        Assert.Equal(NonTerminatorType.ConstDeclaration, constDeclarationLayer4.GetNonTerminatorType());
        Assert.Equal(3, constDeclarationLayer4.Children.Count);
        Assert.Contains(constDeclarationLayer4.Children, node =>
        {
            if (node.IsTerminated && node.GetSemanticToken().TokenType == SemanticTokenType.Identifier)
            {
                return true;
            }

            if (node.IsTerminated && node.GetSemanticToken().TokenType == SemanticTokenType.Operator)
            {
                OperatorSemanticToken token = (OperatorSemanticToken)node.GetSemanticToken();

                return token.OperatorType == OperatorType.Equal;
            }

            if (!node.IsTerminated && node.GetNonTerminatorType() == NonTerminatorType.ConstValue)
            {
                return true;
            }

            return false;
        });


        // ConstValue layer4
        SyntaxNode constValueLayer4 = root.Children.Where(child =>
            !child.IsTerminated && child.GetNonTerminatorType() == NonTerminatorType.ConstValue
        ).ElementAt(0);
        Assert.Equal(NonTerminatorType.ConstValue, constValueLayer4.GetNonTerminatorType());
        Assert.Equal(3, constValueLayer4.Children.Count);
        Assert.Contains(constValueLayer4.Children, node =>
        {
            if (node.IsTerminated && node.GetSemanticToken().TokenType == SemanticTokenType.Delimiter)
            {
                DelimiterSemanticToken token = (DelimiterSemanticToken)node.GetSemanticToken();

                return token.DelimiterType == DelimiterType.SingleQuotation;
            }

            if (node.IsTerminated && node.GetSemanticToken().TokenType == SemanticTokenType.Character)
            {
                return true;
            }

            if (node.IsTerminated && node.GetSemanticToken().TokenType == SemanticTokenType.Delimiter)
            {
                DelimiterSemanticToken token = (DelimiterSemanticToken)node.GetSemanticToken();

                return token.DelimiterType == DelimiterType.SingleQuotation;
            }

            return false;
        });

        // ConstValue layer5
        SyntaxNode constValueLayer5 = constDeclarationLayer4.Children.Where(child =>
            !child.IsTerminated && child.GetNonTerminatorType() == NonTerminatorType.ConstValue
        ).ElementAt(0);
        Assert.Equal(NonTerminatorType.ConstValue, constValueLayer5.GetNonTerminatorType());
        Assert.Single(constValueLayer5.Children);
        Assert.Contains(constValueLayer5.Children, node =>
        {
            if (node.IsTerminated && node.GetSemanticToken().TokenType == SemanticTokenType.Number)
            {
                return true;
            }

            return false;
        });
    }
}
