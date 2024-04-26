using Canon.Core.Abstractions;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;
using Canon.Core.SyntaxNodes;
using Microsoft.Extensions.Logging;

namespace Canon.Core.SemanticParser;

public class TypeCheckVisitor(ILogger<TypeCheckVisitor>? logger = null) : SyntaxNodeVisitor
{
    public SymbolTable SymbolTable { get; private set; } = new();

    public override void PostVisit(ConstValue constValue)
    {
        base.PostVisit(constValue);
        constValue.OnNumberGenerator += (_, e) =>
        {
            switch (e.Token.NumberType)
            {
                case NumberType.Integer:
                    constValue.ConstType = PascalBasicType.Integer;
                    break;
                case NumberType.Real:
                    constValue.ConstType = PascalBasicType.Real;
                    break;
            }
        };

        constValue.OnCharacterGenerator += (_, _) => { constValue.ConstType = PascalBasicType.Character; };
    }

    public override void PostVisit(ConstDeclaration constDeclaration)
    {
        base.PostVisit(constDeclaration);
        (IdentifierSemanticToken token, ConstValue constValue) = constDeclaration.ConstValue;

        bool result = SymbolTable.TryAddSymbol(new Symbol
        {
            Const = true, SymbolName = token.IdentifierName, SymbolType = constValue.ConstType
        });

        if (!result)
        {
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
        factor.OnVariableGenerator += (_, e) =>
        {
            if (SymbolTable.TryGetSymbol(e.Variable.Identifier.IdentifierName, out Symbol? symbol))
            {
                factor.FactorType = symbol.SymbolType;
            }
        };

        // factor -> (expression)
        factor.OnParethnesisGenerator += (_, e) => { factor.FactorType = e.Expression.ExprssionType; };

        // factor -> id (expression_list)
        factor.OnProcedureCallGenerator += (_, e) =>
        {
            if (!SymbolTable.TryGetSymbol(e.ProcedureName.IdentifierName, out Symbol? procedure))
            {
                logger?.LogError("Procedure '{}' does not define.", e.ProcedureName.IdentifierName);
                return;
            }

            if (procedure.SymbolType is not PascalFunctionType functionType)
            {
                logger?.LogError("Identifier '{}' is not a call-able.", procedure.SymbolName);
                return;
            }

            if (functionType.ReturnType == PascalBasicType.Void)
            {
                logger?.LogError("Procedure '{}' returns void.", procedure.SymbolName);
                return;
            }

            factor.FactorType = functionType.ReturnType;

            if (e.Parameters.Expressions.Count != functionType.Parameters.Count)
            {
                logger?.LogError("Procedure '{}' expects {} parameters but {} provided.",
                    procedure.SymbolName,
                    functionType.Parameters.Count,
                    e.Parameters.Expressions.Count);
                return;
            }

            foreach ((Expression expression, PascalParameterType parameterType) in e.Parameters.Expressions.Zip(
                         functionType.Parameters))
            {
                if (expression.ExprssionType != parameterType)
                {
                    logger?.LogError("");
                    return;
                }
            }
        };

        // factor -> factor
        factor.OnNotGenerator += (_, e) =>
        {
            if (e.Factor.FactorType != PascalBasicType.Boolean)
            {
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

            logger?.LogError("Can't calculate");
        };
    }

    public override void PostVisit(Expression expression)
    {
        base.PostVisit(expression);

        expression.OnSimpleExpressionGenerator += (_, e) =>
        {
            expression.ExprssionType = e.SimpleExpression.SimpleExpressionType;
        };

        expression.OnRelationGenerator += (_, _) => { expression.ExprssionType = PascalBasicType.Boolean; };
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
}
