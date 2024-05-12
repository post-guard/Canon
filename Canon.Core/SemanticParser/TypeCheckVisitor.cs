using System.Diagnostics.CodeAnalysis;
using Canon.Core.Abstractions;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;
using Canon.Core.SyntaxNodes;
using Microsoft.Extensions.Logging;
using Expression = Canon.Core.SyntaxNodes.Expression;

namespace Canon.Core.SemanticParser;

public class TypeCheckVisitor(ICompilerLogger? logger = null) : SyntaxNodeVisitor
{
    /// <summary>
    /// 当前遍历阶段的符号表
    /// </summary>
    public SymbolTable SymbolTable { get; private set; } = new();

    /// <summary>
    /// 语法检查中是否发现错误
    /// </summary>
    public bool IsError { get; private set; }

    public override void PreVisit(ConstDeclaration constDeclaration)
    {
        base.PostVisit(constDeclaration);

        (IdentifierSemanticToken token, ConstValue constValue) = constDeclaration.ConstValue;

        // Lookahead 判断常量值的类型
        if (constValue.Children.Count == 1)
        {
            SemanticToken valueToken = constValue.Children[0].Convert<TerminatedSyntaxNode>().Token;

            switch (valueToken.TokenType)
            {
                case SemanticTokenType.Number:
                    NumberSemanticToken numberSemanticToken = valueToken.Convert<NumberSemanticToken>();
                    switch (numberSemanticToken.NumberType)
                    {
                        case NumberType.Integer:
                            constValue.ConstType = PascalBasicType.Integer;
                            break;
                        case NumberType.Real:
                            constValue.ConstType = PascalBasicType.Real;
                            break;
                    }

                    break;
                case SemanticTokenType.Character:
                    constValue.ConstType = PascalBasicType.Character;
                    break;
            }
        }
        else
        {
            NumberSemanticToken numberSemanticToken = constValue.Children[1].Convert<TerminatedSyntaxNode>()
                .Token.Convert<NumberSemanticToken>();

            switch (numberSemanticToken.NumberType)
            {
                case NumberType.Integer:
                    constValue.ConstType = PascalBasicType.Integer;
                    break;
                case NumberType.Real:
                    constValue.ConstType = PascalBasicType.Real;
                    break;
            }
        }

        if (!SymbolTable.TryAddSymbol(new Symbol
            {
                SymbolName = token.IdentifierName, SymbolType = constValue.ConstType, Const = true
            }))
        {
            IsError = true;
            logger?.LogError("Identifier '{}' has been declared twice!", token.IdentifierName);
        }
    }

    public override void PostVisit(Factor factor)
    {
        base.PostVisit(factor);

        // factor -> num
        factor.OnNumberGenerator += (_, e) =>
        {
            switch (e.Token.NumberType)
            {
                case NumberType.Integer:
                    factor.VariableType = PascalBasicType.Integer;
                    break;
                case NumberType.Real:
                    factor.VariableType = PascalBasicType.Real;
                    break;
            }
        };

        // factor -> true | false
        factor.OnBooleanGenerator += (_, _) => { factor.VariableType = PascalBasicType.Boolean; };

        // factor -> variable
        factor.OnVariableGenerator += (_, e) => { factor.VariableType = e.Variable.VariableType; };


        // factor -> (expression)
        factor.OnParethnesisGenerator += (_, e) => { factor.VariableType = e.Expression.VariableType; };

        // factor -> id ( ExpressionList)
        factor.OnProcedureCallGenerator += (_, e) =>
        {
            if (ValidateProcedureCall(e.ProcedureName, e.Parameters, out PascalFunctionType? functionType))
            {
                if (functionType.ReturnType != PascalBasicType.Void)
                {
                    factor.VariableType = functionType.ReturnType;
                    return;
                }
                else
                {
                    IsError = true;
                    logger?.LogError("The procedure '{}' returns void.", e.ProcedureName.IdentifierName);
                }
            }

            factor.VariableType = PascalBasicType.Void;
        };

        // factor -> not factor
        factor.OnNotGenerator += (_, e) => { factor.VariableType = e.Factor.VariableType; };

        // factor -> uminus factor
        factor.OnUminusGenerator += (_, e) => { factor.VariableType = e.Factor.VariableType; };

        // factor -> plus factor
        factor.OnPlusGenerator += (_, e) => { factor.VariableType = e.Factor.VariableType; };
    }

    public override void PostVisit(Term term)
    {
        base.PostVisit(term);

        term.OnFactorGenerator += (_, e) => { term.VariableType = e.Factor.VariableType; };

        term.OnMultiplyGenerator += (_, e) =>
        {
            term.VariableType = e.Left.VariableType + e.Right.VariableType;

            if (term.VariableType == PascalBasicType.Void)
            {
                IsError = true;
                logger?.LogError("Can't calculate");
            }
        };
    }

    public override void PostVisit(SimpleExpression simpleExpression)
    {
        base.PostVisit(simpleExpression);

        simpleExpression.OnTermGenerator += (_, e) => { simpleExpression.VariableType = e.Term.VariableType; };

        simpleExpression.OnAddGenerator += (_, e) =>
        {
            simpleExpression.VariableType = e.Left.VariableType + e.Right.VariableType;

            if (simpleExpression.VariableType == PascalBasicType.Void)
            {
                IsError = true;
                logger?.LogError("Can't calculate");
            }
        };
    }

    public override void PostVisit(Expression expression)
    {
        base.PostVisit(expression);

        expression.OnSimpleExpressionGenerator += (_, e) =>
        {
            expression.VariableType = e.SimpleExpression.VariableType;
        };

        expression.OnRelationGenerator += (_, _) => { expression.VariableType = PascalBasicType.Boolean; };
    }

    public override void PostVisit(TypeSyntaxNode typeSyntaxNode)
    {
        base.PostVisit(typeSyntaxNode);

        typeSyntaxNode.OnBasicTypeGenerator += (_, e) => { typeSyntaxNode.PascalType = e.BasicType.PascalType; };

        typeSyntaxNode.OnArrayTypeGenerator += (_, e) =>
        {
            List<Period> periods = e.Period.Periods;
            (NumberSemanticToken begin, NumberSemanticToken end) = periods.Last().Range;

            PascalType arrayType =
                new PascalArrayType(e.BasicType.PascalType, begin.ParseAsInteger(), end.ParseAsInteger());

            for (int i = periods.Count - 2; i >= 0; i--)
            {
                (begin, end) = periods[i].Range;
                arrayType = new PascalArrayType(arrayType, begin.ParseAsInteger(), end.ParseAsInteger());
            }

            typeSyntaxNode.PascalType = arrayType;
        };
    }

    public override void PreVisit(IdentifierList identifierList)
    {
        base.PreVisit(identifierList);

        identifierList.OnIdentifierGenerator += (_, e) =>
        {
            e.IdentifierList.IsReference = identifierList.IsReference;
            e.IdentifierList.IsProcedure = identifierList.IsProcedure;
        };
    }

    public override void PostVisit(IdentifierList identifierList)
    {
        base.PostVisit(identifierList);

        identifierList.OnTypeGenerator += (_, e) =>
        {
            identifierList.DefinitionType = identifierList.IsReference
                ? e.TypeSyntaxNode.PascalType.ToReferenceType()
                : e.TypeSyntaxNode.PascalType;
        };

        identifierList.OnIdentifierGenerator += (_, e) =>
        {
            identifierList.DefinitionType = e.IdentifierList.DefinitionType;

            Symbol symbol = new()
            {
                SymbolName = e.IdentifierToken.IdentifierName, SymbolType = identifierList.DefinitionType,
            };
            SymbolTable.TryAddSymbol(symbol);

            if (identifierList.IsProcedure)
            {
                _parameters!.Add(symbol);
            }
        };
    }

    public override void PostVisit(VarDeclaration varDeclaration)
    {
        base.PostVisit(varDeclaration);

        SymbolTable.TryAddSymbol(new Symbol
        {
            SymbolName = varDeclaration.Token.IdentifierName,
            SymbolType = varDeclaration.IdentifierList.DefinitionType
        });
    }

    /// <summary>
    /// 存储定义函数过程中的参数列表
    /// 考虑到不同的ValueParameter
    /// ValueParameter的顺序是正确的
    /// 但ValueParameter中的顺序是相反的
    /// 因此设置二维数组存储
    /// </summary>
    private List<Symbol>? _parameters;

    /// <summary>
    /// 多个ValueParameter下定义的参数列表
    /// </summary>
    protected readonly List<List<Symbol>> ValueParameters = [];

    public override void PreVisit(Subprogram subprogram)
    {
        base.PreVisit(subprogram);

        SymbolTable = SymbolTable.CreateChildTable();
    }

    public override void PostVisit(Subprogram subprogram)
    {
        base.PostVisit(subprogram);

        if (!SymbolTable.TryGetParent(out SymbolTable? parent))
        {
            return;
        }

        SymbolTable = parent;
    }

    public override void PreVisit(SubprogramHead subprogramHead)
    {
        base.PreVisit(subprogramHead);

        _parameters = null;
        ValueParameters.Clear();
    }

    public override void PostVisit(SubprogramHead subprogramHead)
    {
        base.PostVisit(subprogramHead);

        // 将当前过程的符号添加到父符号表中
        if (!SymbolTable.TryGetParent(out SymbolTable? parent))
        {
            return;
        }

        List<PascalParameterType> parameters = [];

        // 正序遍历_valueParameter
        // 倒序遍历其中的列表
        foreach (List<Symbol> children in ValueParameters)
        {
            foreach (Symbol symbol in children.AsEnumerable().Reverse())
            {
                parameters.Add(new PascalParameterType(symbol.SymbolType, symbol.SymbolName));
            }
        }

        subprogramHead.OnProcedureGenerator += (_, _) =>
        {
            parent.TryAddSymbol(new Symbol
            {
                SymbolName = subprogramHead.SubprogramName.IdentifierName,
                SymbolType = new PascalFunctionType(parameters, PascalBasicType.Void)
            });
        };

        subprogramHead.OnFunctionGenerator += (_, e) =>
        {
            parent.TryAddSymbol(new Symbol
            {
                SymbolName = subprogramHead.SubprogramName.IdentifierName,
                SymbolType = new PascalFunctionType(parameters, e.ReturnType.PascalType)
            });

            // 在Pascal中返回值是添加一个类型为返回类型 名称为函数名称的局部变量
            SymbolTable.TryAddSymbol(new Symbol
            {
                SymbolName = subprogramHead.SubprogramName.IdentifierName, SymbolType = e.ReturnType.PascalType
            });
        };
    }

    public override void PreVisit(VarParameter varParameter)
    {
        base.PreVisit(varParameter);

        varParameter.ValueParameter.IsReference = true;
    }

    public override void PreVisit(ValueParameter valueParameter)
    {
        base.PreVisit(valueParameter);

        valueParameter.IdentifierList.IsProcedure = true;
        _parameters = [];
        ValueParameters.Add(_parameters);
        if (valueParameter.IsReference)
        {
            valueParameter.IdentifierList.IsReference = true;
        }
    }

    public override void PostVisit(ValueParameter valueParameter)
    {
        base.PostVisit(valueParameter);

        Symbol symbol = new()
        {
            SymbolName = valueParameter.Token.IdentifierName,
            SymbolType = valueParameter.IdentifierList.DefinitionType
        };
        SymbolTable.TryAddSymbol(symbol);

        // 同时添加到参数列表
        _parameters!.Add(symbol);
    }

    public override void PostVisit(Statement statement)
    {
        base.PostVisit(statement);
        // statement -> Variable AssignOp Expression

        statement.OnAssignGenerator += (_, e) =>
        {
            // 检查是否有注册变量
            if (!SymbolTable.TryGetSymbol(e.Variable.Identifier.IdentifierName, out Symbol? variable))
            {
                IsError = true;
                logger?.LogError("Variable '{}' does not define.", e.Variable.Identifier.IdentifierName);
                return;
            }

            // 检查是否为常量
            if (variable.Const)
            {
                IsError = true;
                logger?.LogError("Can't assign value to const '{}'.,",
                    e.Variable.Identifier.IdentifierName);
            }

            if (e.Variable.VariableType != e.Expression.VariableType)
            {
                IsError = true;
                logger?.LogError("Variable '{}' type mismatch, expect '{}' but '{}'.",
                    e.Variable.Identifier.IdentifierName,
                    e.Variable.VariableType.ToString(),
                    e.Expression.VariableType.ToString());
            }
        };

        // statement -> for id AssignOp Expression to Expression do Statement
        statement.OnForGenerator += (_, e) =>
        {
            // 检查id是否存在
            if (!SymbolTable.TryGetSymbol(e.Iterator.IdentifierName, out Symbol? _))
            {
                IsError = true;
                logger?.LogError("Variable '{}' does not define.", e.Iterator.IdentifierName);
                return;
            }

            // 检查ExpressionA是否为Integer
            if (e.Begin.VariableType != PascalBasicType.Integer)
            {
                IsError = true;
                logger?.LogError("The loop begin parameter is not integer.");
                return;
            }

            // 检查ExpressionB是否为Integer
            if (e.End.VariableType != PascalBasicType.Integer)
            {
                IsError = true;
                logger?.LogError("The loop end parameter is not integer.");
            }
        };

        // statement -> if Expression then Statement ElsePart
        statement.OnIfGenerator += (_, e) =>
        {
            // 条件是否为Boolean
            if (e.Condition.VariableType != PascalBasicType.Boolean)
            {
                IsError = true;
                logger?.LogError("Expect '{}' but '{}'.", PascalBasicType.Boolean.TypeName,
                    e.Condition.VariableType.ToString());
            }
        };

        // statement -> while Expression do Statement
        statement.OnWhileGenerator += (_, e) =>
        {
            // 条件是否为Boolean
            if (e.Condition.VariableType != PascalBasicType.Boolean)
            {
                IsError = true;
                logger?.LogError("Expect '{}' but '{}'.", PascalBasicType.Boolean.TypeName,
                    e.Condition.VariableType.ToString());
            }
        };
    }

    public override void PostVisit(ProcedureCall procedureCall)
    {
        base.PostVisit(procedureCall);

        procedureCall.OnParameterGenerator += (_, e) =>
        {
            if (ValidateProcedureCall(procedureCall.ProcedureId, e.Parameters.Expressions,
                    out PascalFunctionType? functionType))
            {
                procedureCall.ReturnType = functionType.ReturnType;
            }
        };

        procedureCall.OnNoParameterGenerator += (_, _) =>
        {
            if (ValidateProcedureCall(procedureCall.ProcedureId, [],
                    out PascalFunctionType? functionType))
            {
                procedureCall.ReturnType = functionType.ReturnType;
            }
        };
    }

    public override void PostVisit(Variable variable)
    {
        base.PostVisit(variable);

        if (SymbolTable.TryGetSymbol(variable.Identifier.IdentifierName, out Symbol? id))
        {
            variable.VariableType = id.SymbolType;

            if (variable.VariableType is PascalFunctionType functionType)
            {
                // 考虑不带参数的函数调用
                if (functionType.Parameters.Count == 0 && functionType.ReturnType != PascalBasicType.Void)
                {
                    variable.VariableType = functionType.ReturnType;
                }
            }

            for (int i = 0; i < variable.VarPart.IndexCount; i++)
            {
                if (variable.VariableType is PascalArrayType arrayType)
                {
                    variable.VariableType = arrayType.ElementType;
                }
                else
                {
                    // 提前不为ArrayType,赋值变量维数写多
                    IsError = true;
                    logger?.LogError("Array dimension mismatch, more than expect");
                    return;
                }
            }
        }
        else
        {
            // 没有注册变量
            IsError = true;
            logger?.LogError("Variable '{}' does not define.", variable.Identifier.IdentifierName);
        }
    }

    public override void PostVisit(IdentifierVarPart identifierVarPart)
    {
        base.PostVisit(identifierVarPart);
        identifierVarPart.OnIndexGenerator += (_, e) =>
        {
            foreach (Expression expression in e.IndexParameters.Expressions)
            {
                if (expression.VariableType != PascalBasicType.Integer)
                {
                    IsError = true;
                    logger?.LogError("Index of array expect 'int' but '{}'",
                        expression.VariableType.ToString());
                }
            }

            identifierVarPart.IndexCount = e.IndexParameters.Expressions.Count;
        };
    }

    private static readonly HashSet<string> s_pascalCoreProcedures =
    [
        "read",
        "readln",
        "write",
        "writeln"
    ];

    /// <summary>
    /// 验证过程调用的类型正确性
    /// </summary>
    /// <param name="procedureName">过程的名称</param>
    /// <param name="parameters">调用过程的参数</param>
    /// <param name="functionType">过程的类型</param>
    /// <returns>是否正确进行调用</returns>
    private bool ValidateProcedureCall(IdentifierSemanticToken procedureName, List<Expression> parameters,
        [NotNullWhen(true)] out PascalFunctionType? functionType)
    {
        if (s_pascalCoreProcedures.Contains(procedureName.IdentifierName))
        {
            functionType = PascalFunctionType.CoreFuntionType;
            return true;
        }

        if (!SymbolTable.TryGetSymbol(procedureName.IdentifierName, out Symbol? symbol))
        {
            functionType = null;
            IsError = true;
            logger?.LogError("Identifier '{}' is not defined.", procedureName.IdentifierName);
            return false;
        }

        PascalFunctionType? targetFunctionType = null;
        if (symbol.SymbolType is not PascalFunctionType pascalFunctionType)
        {
            // 尝试查询父级符号表
            // 处理过程定义中的递归调用问题
            if (SymbolTable.TryGetParent(out SymbolTable? parent))
            {
                if (parent.TryGetSymbol(procedureName.IdentifierName, out Symbol? parentSymbol))
                {
                    if (parentSymbol.SymbolType is PascalFunctionType parentFunctionType)
                    {
                        targetFunctionType = parentFunctionType;
                    }
                }
            }
        }
        else
        {
            targetFunctionType = pascalFunctionType;
        }

        if (targetFunctionType is null)
        {
            functionType = null;
            IsError = true;
            logger?.LogError("Identifier '{}' is not call-able.", procedureName.IdentifierName);
            return false;
        }

        if (targetFunctionType.Parameters.Count != parameters.Count)
        {
            functionType = null;
            IsError = true;
            logger?.LogError("Procedure '{}' needs {} parameters but provide {} parameters.",
                procedureName.IdentifierName,
                targetFunctionType.Parameters.Count, parameters.Count);
            return false;
        }

        foreach ((Expression expression, PascalParameterType parameterType) in parameters.Zip(targetFunctionType
                     .Parameters))
        {
            if (expression.VariableType != parameterType.ParameterType)
            {
                IsError = true;
                logger?.LogError("Parameter expect '{}' but '{}' is provided.",
                    parameterType.ParameterType, expression.VariableType);
            }
        }

        functionType = targetFunctionType;
        return true;
    }
}
