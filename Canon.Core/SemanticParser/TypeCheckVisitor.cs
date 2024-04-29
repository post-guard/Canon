using Canon.Core.Abstractions;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;
using Canon.Core.SyntaxNodes;
using Microsoft.Extensions.Logging;
using Expression = Canon.Core.SyntaxNodes.Expression;

namespace Canon.Core.SemanticParser;

public class TypeCheckVisitor(ICompilerLogger? logger = null) : SyntaxNodeVisitor
{
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
                Const = true, SymbolName = token.IdentifierName, SymbolType = constValue.ConstType
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
                    factor.FactorType = PascalBasicType.Integer;
                    break;
                case NumberType.Real:
                    factor.FactorType = PascalBasicType.Real;
                    break;
            }
        };

        // factor -> variable
        factor.OnVariableGenerator += (_, e) => { factor.FactorType = e.Variable.VariableType; };

        // factor -> (expression)
        factor.OnParethnesisGenerator += (_, e) => { factor.FactorType = e.Expression.ExpressionType; };

        // factor -> id (expression_list)
        factor.OnProcedureCallGenerator += (_, e) =>
        {
            if (!SymbolTable.TryGetSymbol(e.ProcedureName.IdentifierName, out Symbol? procedure))
            {
                IsError = true;
                logger?.LogError("Procedure '{}' does not define.", e.ProcedureName.IdentifierName);
                return;
            }

            PascalFunctionType? functionType = procedure.SymbolType as PascalFunctionType;
            if (functionType is null)
            {
                if (SymbolTable.TryGetParent(out SymbolTable? parent))
                {
                    if (parent.TryGetSymbol(e.ProcedureName.IdentifierName, out procedure))
                    {
                        functionType = procedure.SymbolType as PascalFunctionType;
                    }
                }
            }

            if (functionType is null)
            {
                IsError = true;
                logger?.LogError("'{}' is not call able.", e.ProcedureName.IdentifierName);
                return;
            }

            if (functionType.ReturnType == PascalBasicType.Void)
            {
                IsError = true;
                logger?.LogError("Procedure '{}' returns void.", e.ProcedureName.IdentifierName);
                return;
            }

            factor.FactorType = functionType.ReturnType;

            if (e.Parameters.Expressions.Count != functionType.Parameters.Count)
            {
                IsError = true;
                logger?.LogError("Procedure '{}' expects {} parameters but {} provided.",
                    e.ProcedureName.IdentifierName,
                    functionType.Parameters.Count,
                    e.Parameters.Expressions.Count);
                return;
            }

            foreach ((Expression expression, PascalParameterType parameterType) in e.Parameters.Expressions.Zip(
                         functionType.Parameters))
            {
                if (expression.ExpressionType != parameterType.ParameterType)
                {
                    IsError = true;
                    logger?.LogError("Parameter expect '{}' but '{}'",
                        parameterType.ParameterType.TypeName, expression.ExpressionType);
                    return;
                }
            }
        };

        // factor -> factor
        factor.OnNotGenerator += (_, e) =>
        {
            if (e.Factor.FactorType != PascalBasicType.Boolean)
            {
                IsError = true;
                logger?.LogError("The boolean type is expected.");
                return;
            }

            factor.FactorType = PascalBasicType.Boolean;
        };

        // factor -> uminus factor
        factor.OnUminusGenerator += (_, e) => { factor.FactorType = e.Factor.FactorType; };
    }

    public override void PostVisit(Term term)
    {
        base.PostVisit(term);

        term.OnFactorGenerator += (_, e) => { term.TermType = e.Factor.FactorType; };

        term.OnMultiplyGenerator += (_, e) =>
        {
            if (PascalType.IsCalculatable(e.Left.TermType) && PascalType.IsCalculatable(e.Right.FactorType))
            {
                term.TermType = e.Left.TermType + e.Right.FactorType;
                return;
            }

            IsError = true;
            logger?.LogError("Can't calculate");
        };
    }

    public override void PostVisit(SimpleExpression simpleExpression)
    {
        base.PostVisit(simpleExpression);

        simpleExpression.OnTermGenerator += (_, e) => { simpleExpression.SimpleExpressionType = e.Term.TermType; };

        simpleExpression.OnAddGenerator += (_, e) =>
        {
            if (PascalType.IsCalculatable(e.Left.SimpleExpressionType) && PascalType.IsCalculatable(e.Right.TermType))
            {
                simpleExpression.SimpleExpressionType = e.Left.SimpleExpressionType + e.Right.TermType;
                return;
            }

            IsError = true;
            logger?.LogError("Can't calculate");
        };
    }

    public override void PostVisit(Expression expression)
    {
        base.PostVisit(expression);

        expression.OnSimpleExpressionGenerator += (_, e) =>
        {
            expression.ExpressionType = e.SimpleExpression.SimpleExpressionType;
        };

        expression.OnRelationGenerator += (_, _) => { expression.ExpressionType = PascalBasicType.Boolean; };
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

        identifierList.OnTypeGenerator += (_, e) => { identifierList.DefinitionType = e.TypeSyntaxNode.PascalType; };

        identifierList.OnIdentifierGenerator += (_, e) =>
        {
            identifierList.DefinitionType = e.IdentifierList.DefinitionType;

            Symbol symbol = new()
            {
                SymbolName = e.IdentifierToken.IdentifierName,
                SymbolType = identifierList.DefinitionType,
                Reference = identifierList.IsReference
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
    private readonly List<List<Symbol>> _valueParameters = [];

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
        _valueParameters.Clear();
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
        foreach (List<Symbol> children in _valueParameters)
        {
            foreach (Symbol symbol in children.AsEnumerable().Reverse())
            {
                parameters.Add(new PascalParameterType(symbol.SymbolType, symbol.Reference));
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
        _valueParameters.Add(_parameters);
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
            SymbolType = valueParameter.IdentifierList.DefinitionType,
            Reference = valueParameter.IsReference
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

            if (e.Variable.VariableType != e.Expression.ExpressionType)
            {
                IsError = true;
                logger?.LogError("Variable '{}' type mismatch, expect '{}' but '{}'.",
                    e.Variable.Identifier.IdentifierName,
                    e.Variable.VariableType.ToString(),
                    e.Expression.ExpressionType.ToString());
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
            if (e.Begin.ExpressionType != PascalBasicType.Integer)
            {
                IsError = true;
                logger?.LogError("The loop begin parameter is not integer.");
                return;
            }

            // 检查ExpressionB是否为Integer
            if (e.End.ExpressionType != PascalBasicType.Integer)
            {
                IsError = true;
                logger?.LogError("The loop end parameter is not integer.");
            }
        };

        // statement -> if Expression then Statement ElsePart
        statement.OnIfGenerator += (_, e) =>
        {
            // 条件是否为Boolean
            if (e.Condition.ExpressionType != PascalBasicType.Boolean)
            {
                IsError = true;
                logger?.LogError("Expect '{}' but '{}'.", PascalBasicType.Boolean.TypeName,
                    e.Condition.ExpressionType.ToString());
            }
        };
    }

    public override void PostVisit(ProcedureCall procedureCall)
    {
        base.PostVisit(procedureCall);
        // 查看当前符号表中procedureId是否注册

        if (!SymbolTable.TryGetSymbol(procedureCall.ProcedureId.IdentifierName, out Symbol? procedure))
        {
            // id没有定义
            IsError = true;
            logger?.LogError("procedure '{}' is not defined.", procedureCall.ProcedureId.LiteralValue);
            return;
        }

        if (procedure.SymbolType is not PascalFunctionType functionType)
        {
            // id不是函数类型,要找父符号表
            if (!SymbolTable.TryGetParent(out SymbolTable? parent))
            {
                // 没找到父符号表,说明这个id是非函数变量
                IsError = true;
                logger?.LogError("Identifier '{}' is not a call-able.", procedureCall.ProcedureId.LiteralValue);
                return;
            }

            if (!parent.TryGetSymbol(procedureCall.ProcedureId.IdentifierName, out Symbol? procedureParent))
            {
                // 找到父符号表但没有找到该id,说明这个id没定义
                IsError = true;
                logger?.LogError("procedure '{}' is not defined.", procedureCall.ProcedureId.LiteralValue);
                return;
            }

            // 父符号表中找到该id
            if (procedureParent.SymbolType is not PascalFunctionType functionTypeParent)
            {
                // 该符号不是函数类型
                IsError = true;
                logger?.LogError("Identifier '{}' is not a call-able.", procedureParent.SymbolName);
                return;
            }
            // 该符号是函数类型,赋给procedure
            procedure = procedureParent;
            functionType = functionTypeParent;
        }

        procedureCall.OnParameterGenerator += (_, e) =>
        {
            // 检查procedure输入参数个数是否相符
            if (e.Parameters.Expressions.Count != functionType.Parameters.Count)
            {
                IsError = true;
                logger?.LogError("Procedure '{}' expects {} parameters but {} provided.",
                    procedure.SymbolName,
                    functionType.Parameters.Count,
                    e.Parameters.Expressions.Count);
                return;
            }

            // 检查每个参数的类型与procedure参数定义类型是否相符
            foreach ((Expression expression, PascalParameterType parameterType) in e.Parameters.Expressions.Zip(
                         functionType.Parameters))
            {
                if (expression.ExpressionType != parameterType.ParameterType)
                {
                    IsError = true;
                    logger?.LogError("Parameter expect '{}' but '{}'",
                        parameterType.ParameterType.TypeName, expression.ExpressionType);
                    return;
                }
            }
        };
    }

    public override void PostVisit(Variable variable)
    {
        base.PostVisit(variable);
        if (SymbolTable.TryGetSymbol(variable.Identifier.IdentifierName, out Symbol? id))
        {
            variable.VariableType = id.SymbolType;

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
                if (expression.ExpressionType != PascalBasicType.Integer)
                {
                    IsError = true;
                    logger?.LogError("Index of array expect 'int' but '{}'",
                        expression.ExpressionType.ToString());
                }
            }

            identifierVarPart.IndexCount = e.IndexParameters.Expressions.Count;
        };
    }
}
