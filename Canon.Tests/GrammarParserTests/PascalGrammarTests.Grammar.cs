using Canon.Core.Enums;
using Canon.Core.GrammarParser;

namespace Canon.Tests.GrammarParserTests;

public partial class PascalGrammarTests
{
    private static readonly Dictionary<NonTerminator, List<List<TerminatorBase>>> s_pascalGrammar = new()
    {
        {
            // ProgramStart -> ProgramStruct
            new NonTerminator(NonTerminatorType.StartNonTerminator), [
                [new NonTerminator(NonTerminatorType.ProgramStruct)]
            ]
        },
        {
            // ProgramStruct -> ProgramHead ; ProgramBody .
            new NonTerminator(NonTerminatorType.ProgramStruct), [
                [
                    new NonTerminator(NonTerminatorType.ProgramHead),
                    new Terminator(DelimiterType.Semicolon),
                    new NonTerminator(NonTerminatorType.ProgramBody),
                    new Terminator(DelimiterType.Period)
                ]
            ]
        },
        {
            // ProgramHead -> program id (IdList) | program id
            new NonTerminator(NonTerminatorType.ProgramHead), [
                [
                    new Terminator(KeywordType.Program),
                    Terminator.IdentifierTerminator,
                    new Terminator(DelimiterType.LeftParenthesis),
                    new NonTerminator(NonTerminatorType.IdentifierList),
                    new Terminator(DelimiterType.RightParenthesis),
                ],
                [
                    new Terminator(KeywordType.Program),
                    Terminator.IdentifierTerminator,
                ]
            ]
        },
        {
            // ProgramBody -> ConstDeclarations
            //            VarDeclarations
            //            SubprogramDeclarations
            //            CompoundStatement
            new NonTerminator(NonTerminatorType.ProgramBody), [
                [
                    new NonTerminator(NonTerminatorType.ConstDeclarations),
                    new NonTerminator(NonTerminatorType.VarDeclarations),
                    new NonTerminator(NonTerminatorType.SubprogramDeclarations),
                    new NonTerminator(NonTerminatorType.CompoundStatement)
                ]
            ]
        },
        {
            // IdList -> id | IdList , id
            new NonTerminator(NonTerminatorType.IdentifierList), [
                [
                    Terminator.IdentifierTerminator,
                ],
                [
                    new NonTerminator(NonTerminatorType.IdentifierList),
                    new Terminator(DelimiterType.Comma),
                    Terminator.IdentifierTerminator
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
        },
        {
            // VarDeclarations -> ε | var VarDeclaration ;
            new NonTerminator(NonTerminatorType.VarDeclarations), [
                [
                    Terminator.EmptyTerminator
                ],
                [
                    new Terminator(KeywordType.Var),
                    new NonTerminator(NonTerminatorType.VarDeclaration),
                    new Terminator(DelimiterType.Semicolon)
                ]
            ]
        },
        {
            // VarDeclaration -> IdList : Type | VarDeclaration ; IdList : Type
            new NonTerminator(NonTerminatorType.VarDeclaration), [
                [
                    new NonTerminator(NonTerminatorType.IdentifierList),
                    new Terminator(DelimiterType.Colon),
                    new NonTerminator(NonTerminatorType.Type)
                ],
                [
                    new NonTerminator(NonTerminatorType.VarDeclaration),
                    new Terminator(DelimiterType.Semicolon),
                    new NonTerminator(NonTerminatorType.IdentifierList),
                    new Terminator(DelimiterType.Colon),
                    new NonTerminator(NonTerminatorType.Type)
                ]
            ]
        },
        {
            // Type -> BasicType | Array [ Period ] of BasicType
            new NonTerminator(NonTerminatorType.Type), [
                [
                    new NonTerminator(NonTerminatorType.BasicType)
                ],
                [
                    new Terminator(KeywordType.Array),
                    new Terminator(DelimiterType.LeftSquareBracket),
                    new NonTerminator(NonTerminatorType.Period),
                    new Terminator(DelimiterType.RightSquareBracket),
                    new Terminator(KeywordType.Of),
                    new NonTerminator(NonTerminatorType.BasicType)
                ]
            ]
        },
        {
            // BasicType -> Integer | Real | Boolean | char
            new NonTerminator(NonTerminatorType.BasicType), [
                [
                    new Terminator(KeywordType.Integer)
                ],
                [
                    new Terminator(KeywordType.Real)
                ],
                [
                    new Terminator(KeywordType.Boolean)
                ],
                [
                    new Terminator(KeywordType.Character)
                ]
            ]
        },
        {
            // Period -> digits .. digits | Period , digits .. digits
            new NonTerminator(NonTerminatorType.Period), [
                [
                    Terminator.NumberTerminator,
                    new Terminator(DelimiterType.DoubleDots),
                    Terminator.NumberTerminator,
                ],
                [
                    new NonTerminator(NonTerminatorType.Period),
                    new Terminator(DelimiterType.Comma),
                    Terminator.NumberTerminator,
                    new Terminator(DelimiterType.DoubleDots),
                    Terminator.NumberTerminator,
                ]
            ]
        },
        {
            // SubprogramDeclarations -> ε | SubprogramDeclarations Subprogram ;
            new NonTerminator(NonTerminatorType.SubprogramDeclarations), [
                [
                    Terminator.EmptyTerminator
                ],
                [
                    new NonTerminator(NonTerminatorType.SubprogramDeclarations),
                    new NonTerminator(NonTerminatorType.Subprogram),
                    new Terminator(DelimiterType.Semicolon)
                ]
            ]
        },
        {
            // Subprogram -> SubprogramHead ; SubprogramBody
            new NonTerminator(NonTerminatorType.Subprogram), [
                [
                    new NonTerminator(NonTerminatorType.SubprogramHead),
                    new Terminator(DelimiterType.Semicolon),
                    new NonTerminator(NonTerminatorType.SubprogramBody)
                ]
            ]
        },
        {
            // SubprogramHead -> procedure id FormalParameter
            //                | function id FormalParameter : BasicType
            new NonTerminator(NonTerminatorType.SubprogramHead), [
                [
                    new Terminator(KeywordType.Procedure),
                    Terminator.IdentifierTerminator,
                    new NonTerminator(NonTerminatorType.FormalParameter)
                ],
                [
                    new Terminator(KeywordType.Function),
                    Terminator.IdentifierTerminator,
                    new NonTerminator(NonTerminatorType.FormalParameter),
                    new Terminator(DelimiterType.Colon),
                    new NonTerminator(NonTerminatorType.BasicType)
                ]
            ]
        },
        {
            // FormalParameter -> ε | ( ParameterList )
            new NonTerminator(NonTerminatorType.FormalParameter), [
                [
                    Terminator.EmptyTerminator,
                ],
                [
                    new Terminator(DelimiterType.LeftParenthesis),
                    new NonTerminator(NonTerminatorType.ParameterList),
                    new Terminator(DelimiterType.RightParenthesis)
                ]
            ]
        },
        {
            // ParameterList -> Parameter | ParameterList ; Parameter
            new NonTerminator(NonTerminatorType.ParameterList), [
                [
                    new NonTerminator(NonTerminatorType.Parameter)
                ],
                [
                    new NonTerminator(NonTerminatorType.ParameterList),
                    new Terminator(DelimiterType.Semicolon),
                    new NonTerminator(NonTerminatorType.Parameter)
                ]
            ]
        },
        {
            // Parameter -> VarParameter | ValueParameter
            new NonTerminator(NonTerminatorType.Parameter), [
                [
                    new NonTerminator(NonTerminatorType.VarParameter)
                ],
                [
                    new NonTerminator(NonTerminatorType.ValueParameter)
                ]
            ]
        },
        {
            // VarParameter -> var ValueParameter
            new NonTerminator(NonTerminatorType.VarParameter), [
                [
                    new Terminator(KeywordType.Var),
                    new NonTerminator(NonTerminatorType.ValueParameter)
                ]
            ]
        },
        {
            // ValueParameter -> IdList : BasicType
            new NonTerminator(NonTerminatorType.ValueParameter), [
                [
                    new NonTerminator(NonTerminatorType.IdentifierList),
                    new Terminator(DelimiterType.Colon),
                    new NonTerminator(NonTerminatorType.BasicType)
                ]
            ]
        },
        {
            // SubprogramBody -> ConstDeclarations
            //                   VarDeclarations
            //                   CompoundStatement
            new NonTerminator(NonTerminatorType.SubprogramBody), [
                [
                    new NonTerminator(NonTerminatorType.ConstDeclarations),
                    new NonTerminator(NonTerminatorType.VarDeclarations),
                    new NonTerminator(NonTerminatorType.CompoundStatement)
                ]
            ]
        },
        {
            // CompoundStatement -> begin StatementList end
            new NonTerminator(NonTerminatorType.CompoundStatement), [
                [
                    new Terminator(KeywordType.Begin),
                    new NonTerminator(NonTerminatorType.StatementList),
                    new Terminator(KeywordType.End)
                ]
            ]
        },
        {
            // StatementList -> Statement | StatementList ; Statement
            new NonTerminator(NonTerminatorType.StatementList), [
                [
                    new NonTerminator(NonTerminatorType.Statement)
                ],
                [
                    new NonTerminator(NonTerminatorType.StatementList),
                    new Terminator(DelimiterType.Semicolon),
                    new NonTerminator(NonTerminatorType.Statement)
                ]
            ]
        },
        {
            // Statement -> ε
            //           | Variable AssignOp Expression
            //           | FuncId AssignOp Expression
            //           | ProcedureCall
            //           | CompoundStatement
            //           | if Expression then Statement ElsePart
            //           | for id AssignOp Expression to Expression do Statement
            //           | read ( VariableList )
            //           | write( ExpressionList )
            // 注意这里 read 和 write 作为普通的函数调用处理了
            // 因此下面并没有单独声明
            new NonTerminator(NonTerminatorType.Statement), [
                [
                    // ε
                    Terminator.EmptyTerminator,
                ],
                [
                    // Variable AssignOp Expression
                    new NonTerminator(NonTerminatorType.Variable),
                    new Terminator(OperatorType.Assign),
                    new NonTerminator(NonTerminatorType.Expression)
                ],
                [
                    // FuncId AssignOp Expression
                    Terminator.IdentifierTerminator,
                    new Terminator(OperatorType.Assign),
                    new NonTerminator(NonTerminatorType.Expression)
                ],
                [
                    // ProcedureCall
                    new NonTerminator(NonTerminatorType.ProcedureCall)
                ],
                [
                    // CompoundStatement
                    new NonTerminator(NonTerminatorType.CompoundStatement)
                ],
                [
                    // if Expression then Statement ElsePart
                    new Terminator(KeywordType.If),
                    new NonTerminator(NonTerminatorType.Expression),
                    new Terminator(KeywordType.Then),
                    new NonTerminator(NonTerminatorType.Statement),
                    new NonTerminator(NonTerminatorType.ElsePart)
                ],
                [
                    // for id AssignOp Expression to Expression do Statement
                    new Terminator(KeywordType.For),
                    Terminator.IdentifierTerminator,
                    new Terminator(OperatorType.Assign),
                    new NonTerminator(NonTerminatorType.Expression),
                    new Terminator(KeywordType.To),
                    new NonTerminator(NonTerminatorType.Expression),
                    new Terminator(KeywordType.Do),
                    new NonTerminator(NonTerminatorType.Statement)
                ]
            ]
        },
        // {
        //     // VariableList -> Variable | VariableList , Variable
        //     // 这里用expressionList代替VariableList
        //     new NonTerminator(NonTerminatorType.ExpressionList), [
        //         [
        //             new NonTerminator(NonTerminatorType.Variable)
        //         ],
        //         [
        //             new NonTerminator(NonTerminatorType.ExpressionList),
        //             new Terminator(DelimiterType.Comma),
        //             new NonTerminator(NonTerminatorType.Variable)
        //         ]
        //     ]
        // },
        {
            // Variable -> id IdVarPart
            new NonTerminator(NonTerminatorType.Variable), [
                [
                    Terminator.IdentifierTerminator,
                    new NonTerminator(NonTerminatorType.IdVarPart)
                ]
            ]
        },
        {
            // IdVarPart -> ε | [ ExpressionList ]
            new NonTerminator(NonTerminatorType.IdVarPart), [
                [
                    Terminator.EmptyTerminator,
                ],
                [
                    new Terminator(DelimiterType.LeftSquareBracket),
                    new NonTerminator(NonTerminatorType.ExpressionList),
                    new Terminator(DelimiterType.RightSquareBracket)
                ]
            ]
        },
        {
            // ProcedureCall -> id | id ( ExpressionList )
            new NonTerminator(NonTerminatorType.ProcedureCall), [
                [
                    Terminator.IdentifierTerminator,
                ],
                [
                    Terminator.IdentifierTerminator,
                    new Terminator(DelimiterType.LeftParenthesis),
                    new NonTerminator(NonTerminatorType.ExpressionList),
                    new Terminator(DelimiterType.RightParenthesis)
                ]
            ]
        },
        {
            // ElsePart -> ε | else statement
            new NonTerminator(NonTerminatorType.ElsePart), [
                [
                    Terminator.EmptyTerminator,
                ],
                [
                    new Terminator(KeywordType.Else),
                    new NonTerminator(NonTerminatorType.Statement)
                ]
            ]
        },
        {
            // ExpressionList -> Expression | ExpressionList , Expression
            new NonTerminator(NonTerminatorType.ExpressionList), [
                [
                    new NonTerminator(NonTerminatorType.Expression)
                ],
                [
                    new NonTerminator(NonTerminatorType.ExpressionList),
                    new Terminator(DelimiterType.Comma),
                    new NonTerminator(NonTerminatorType.Expression)
                ]
            ]
        },
        {
            // Expression -> SimpleExpression | SimpleExpression RelationOperator SimpleExpression
            new NonTerminator(NonTerminatorType.Expression), [
                [
                    new NonTerminator(NonTerminatorType.SimpleExpression)
                ],
                [
                    new NonTerminator(NonTerminatorType.SimpleExpression),
                    new NonTerminator(NonTerminatorType.RelationOperator),
                    new NonTerminator(NonTerminatorType.SimpleExpression)
                ]
            ]
        },
        {
            // SimpleExpression -> Term | SimpleExpression AddOperator Term
            new NonTerminator(NonTerminatorType.SimpleExpression), [
                [
                    new NonTerminator(NonTerminatorType.Term)
                ],
                [
                    new NonTerminator(NonTerminatorType.SimpleExpression),
                    new NonTerminator(NonTerminatorType.AddOperator),
                    new NonTerminator(NonTerminatorType.Term)
                ]
            ]
        },
        {
            // Term -> Factor | Term MultiplyOperator Factor
            new NonTerminator(NonTerminatorType.Term), [
                [
                    new NonTerminator(NonTerminatorType.Factor)
                ],
                [
                    new NonTerminator(NonTerminatorType.Term),
                    new NonTerminator(NonTerminatorType.MultiplyOperator),
                    new NonTerminator(NonTerminatorType.Factor)
                ]
            ]
        },
        {
            // Factor -> num | Variable
            //         | ( Expression )
            //         | id ( ExpressionList )
            //         | not Factor
            //         | minus Factor
            new NonTerminator(NonTerminatorType.Factor), [
                [
                    Terminator.NumberTerminator,
                ],
                [
                    new NonTerminator(NonTerminatorType.Variable)
                ],
                [
                    new Terminator(DelimiterType.LeftParenthesis),
                    new NonTerminator(NonTerminatorType.Expression),
                    new Terminator(DelimiterType.RightParenthesis)
                ],
                [
                    Terminator.IdentifierTerminator,
                    new Terminator(DelimiterType.LeftParenthesis),
                    new NonTerminator(NonTerminatorType.ExpressionList),
                    new Terminator(DelimiterType.RightParenthesis)
                ],
                [
                    new Terminator(KeywordType.Not),
                    new NonTerminator(NonTerminatorType.Factor)
                ],
                [
                    new Terminator(OperatorType.Minus),
                    new NonTerminator(NonTerminatorType.Factor)
                ]
            ]
        },
        {
            // AddOperator -> + | - | or
            new NonTerminator(NonTerminatorType.AddOperator), [
                [
                    new Terminator(OperatorType.Plus)
                ],
                [
                    new Terminator(OperatorType.Minus)
                ],
                [
                    new Terminator(KeywordType.Or)
                ]
            ]
        },
        {
            // MultiplyOperator -> * | / | div | mod | and
            new NonTerminator(NonTerminatorType.MultiplyOperator), [
                [
                    new Terminator(OperatorType.Multiply),
                ],
                [
                    new Terminator(OperatorType.Divide),
                ],
                [
                    new Terminator(KeywordType.Divide)
                ],
                [
                    new Terminator(KeywordType.Mod)
                ],
                [
                    new Terminator(KeywordType.And)
                ]
            ]
        },
        {
            // RelationOperator -> = | <> | < | <= | > | >=
            new NonTerminator(NonTerminatorType.RelationOperator), [
                [
                    new Terminator(OperatorType.Equal)
                ],
                [
                    new Terminator(OperatorType.NotEqual)
                ],
                [
                    new Terminator(OperatorType.Less)
                ],
                [
                    new Terminator(OperatorType.LessEqual)
                ],
                [
                    new Terminator(OperatorType.Greater)
                ],
                [
                    new Terminator(OperatorType.GreaterEqual)
                ]
            ]
        }
    };
}
