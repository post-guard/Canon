using System.Globalization;
using Canon.Core.Abstractions;
using Canon.Core.CodeGenerators;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;
using Canon.Core.SyntaxNodes;
using BasicType = Canon.Core.SyntaxNodes.BasicType;

namespace Canon.Core.SemanticParser;

public class CCodeGenerateVisitor(ICompilerLogger? logger = null) : TypeCheckVisitor(logger)
{
    public CCodeBuilder Builder { get; } = new();

    public override void PreVisit(ProgramStruct programStruct)
    {
        base.PreVisit(programStruct);

        Builder.AddString("#include <stdbool.h>\n");
        Builder.AddString("#include <stdio.h>\n");
    }

    private string? _constValue;
    public override void PreVisit(ConstDeclaration constDeclaration)
    {
        base.PreVisit(constDeclaration);
        (_, ConstValue constValue) = constDeclaration.ConstValue;
        constValue.OnCharacterGenerator += (_, e) =>
        {
            _constValue = "'" + e.Token.LiteralValue + "'";
        };
        constValue.OnNumberGenerator += (_, e) =>
        {
            if (e.IsNegative)
            {
                _constValue = "-";
            }

            if (e.Token.NumberType == NumberType.Integer)
            {
                _constValue += e.Token.ParseAsInteger();
            }
            else
            {
                _constValue += e.Token.ParseAsReal();
            }
        };
    }

    public override void PostVisit(ConstDeclaration constDeclaration)
    {
        base.PostVisit(constDeclaration);
        (IdentifierSemanticToken token, ConstValue constValue) = constDeclaration.ConstValue;

        string cTypeName = TryParseBasicType(constValue.ConstType);
        Builder.AddString("const " + cTypeName + " " + token.IdentifierName + " = " + _constValue + ";\n");
        _constValue = "";
    }

    public override void PostVisit(VarDeclaration varDeclaration)
    {
        base.PostVisit(varDeclaration);

        string idName = varDeclaration.Token.IdentifierName;
        var type = varDeclaration.IdentifierList.DefinitionType;

        if (type is PascalBasicType)
        {
            Builder.AddString(idName + ";\n");
        }
        else
        {
            TryParseArrayType(type, out string _, out string periods);
            Builder.AddString(idName + periods + ";\n");
        }

    }

    public override void PreVisit(IdentifierList identifierList)
    {
        base.PreVisit(identifierList);
        identifierList.OnTypeGenerator += (_, e) =>
        {
            e.TypeSyntaxNode.IsProcedure = identifierList.IsProcedure;
        };
    }

    public override void PostVisit(IdentifierList identifierList)
    {
        base.PostVisit(identifierList);
        identifierList.OnIdentifierGenerator += (_, e) =>
        {
            if(!identifierList.IsProcedure)
            {
                string periods = ""; //如果是数组定义，需要每个标识符后带上
                if (e.IdentifierList.DefinitionType is PascalArrayType pascalArrayType)
                {
                    TryParseArrayType(pascalArrayType, out string _, out string periods0);
                    periods = periods0;
                }

                Builder.AddString(e.IdentifierToken.IdentifierName + periods + ", ");
            }
        };
    }


    public override void PreVisit(TypeSyntaxNode typeSyntaxNode)
    {
        base.PreVisit(typeSyntaxNode);
        typeSyntaxNode.OnBasicTypeGenerator += (_, e) =>
        {
            e.BasicType.IsProcedure = typeSyntaxNode.IsProcedure;
        };
        typeSyntaxNode.OnArrayTypeGenerator += (_, e) =>
        {
            e.BasicType.IsProcedure = typeSyntaxNode.IsProcedure;
        };
    }

    public override void PostVisit(BasicType basicType)
    {
        base.PostVisit(basicType);
        if (!basicType.IsProcedure)
        {
            Builder.AddString(TryParseBasicType(basicType.PascalType) + " ");
        }
    }


    public override void PostVisit(SubprogramHead subprogramHead)
    {
        base.PostVisit(subprogramHead);

        string subprogramName = subprogramHead.SubprogramName.IdentifierName;
        //只需特判过程，函数的返回值类型已在basicType节点上生成
        if (subprogramHead.IsProcedure)
        {
            Builder.AddString("void ");
        }
        Builder.AddString(subprogramName);
        //生成参数列表
        Builder.AddString("(");
        List<string> parametersInfo = new();

        foreach (List<Symbol> children in ValueParameters)
        {
            foreach (Symbol symbol in children.AsEnumerable().Reverse())
            {
                if (symbol.SymbolType is PascalBasicType pascalType)
                {
                    string typeName = TryParseBasicType(pascalType);
                    if (symbol.Reference)
                    {
                        parametersInfo.Add(typeName + "* " + symbol.SymbolName);
                    }
                    else
                    {
                        parametersInfo.Add(typeName + " " + symbol.SymbolName);
                    }
                }
                else
                {
                    TryParseArrayType(symbol.SymbolType, out string basicTypeName, out string periods);
                    parametersInfo.Add(string.Concat(basicTypeName, " ", symbol.SymbolName, "[]",
                        periods.Substring(periods.IndexOf(']') + 1)));
                }
            }
        }
        Builder.AddString(string.Join(", ", parametersInfo));
        Builder.AddString(")\n");
    }

    private string _subprogramName = "";
    public override void PreVisit(Subprogram subprogram)
    {
        base.PreVisit(subprogram);
        _subprogramName = subprogram.Head.SubprogramName.IdentifierName;
    }

    public override void PostVisit(Subprogram subprogram)
    {
        base.PostVisit(subprogram);
        if (subprogram.Head.IsProcedure)
        {
            Builder.AddString("\n}\n");
        }
        else
        {
            //为函数生成返回语句
            Builder.AddString("return " + subprogram.Head.SubprogramName.IdentifierName + ";\n}\n");
        }
    }

    public override void PreVisit(SubprogramBody subprogramBody)
    {
        base.PreVisit(subprogramBody);
        Builder.AddString("{\n");
        //生成函数返回值变量
        SymbolTable.TryGetSymbol(_subprogramName, out var symbol);
        if (symbol is null || symbol.SymbolType is not PascalBasicType)
        {
            return;
        }

        Builder.AddString(TryParseBasicType(symbol.SymbolType.Convert<PascalType>()) + " " + _subprogramName + ";\n");
    }

    public override void PreVisit(ProcedureCall procedureCall)
    {
        base.PreVisit(procedureCall);
        string procedureName = procedureCall.ProcedureId.IdentifierName;

        if (procedureName == "read")
        {
            Builder.AddString("scanf(\"%d\", &");
            return;
        }
        if (procedureName == "write")
        {
            Builder.AddString("printf(\"%d\", ");
            return;
        }

        Builder.AddString(procedureName + "(");

        procedureCall.OnParameterGenerator += (_, e) =>
        {
            string procedureIdName = procedureCall.ProcedureId.IdentifierName;

            SymbolTable.TryGetParent(out var parentTable);
            parentTable ??= SymbolTable;

            parentTable.TryGetSymbol(procedureIdName, out var symbol);

            if (symbol is null)
            {
                return;
            }
            e.Parameters.ParameterTypes.AddRange(symbol.SymbolType.Convert<PascalFunctionType>().Parameters);
            e.Parameters.Expression.LastParam = true;
            e.Parameters.IsParamList = true;
        };
    }

    public override void PostVisit(ProcedureCall procedureCall)
    {
        base.PostVisit(procedureCall);
        Builder.AddString(")");
    }

    public override void PreVisit(ProgramBody programBody)
    {
        base.PreVisit(programBody);
        //当子函数全部定义完成时，生成main函数头
        programBody.CompoundStatement.IsMain = true;
    }

    public override void PostVisit(CompoundStatement compoundStatement)
    {
        base.PostVisit(compoundStatement);
        if (compoundStatement.IsMain)
        {
            Builder.AddString("\nreturn 0;\n");
        }
        Builder.AddString("}\n");
    }

    public override void PreVisit(CompoundStatement compoundStatement)
    {
        base.PreVisit(compoundStatement);
        if (compoundStatement.IsMain)
        {
            Builder.AddString("int main()\n");
        }
        Builder.AddString("{\n");
    }

    public override void PreVisit(Statement statement)
    {
        base.PreVisit(statement);
        statement.OnForGenerator += (_, e) =>
        {
            e.Begin.Iterator = e.Iterator;
            e.Begin.IsForConditionBegin = true;
            e.End.Iterator = e.Iterator;
            e.End.IsForConditionEnd = true;
        };
        statement.OnAssignGenerator += (_, e) =>
        {
            e.Expression.IsAssign = true;
        };
    }

    public override void PostVisit(Statement statement)
    {
        base.PostVisit(statement);
        Builder.AddString(";\n");
    }

    public override void PreVisit(Variable variable)
    {
        base.PreVisit(variable);

        SymbolTable.TryGetSymbol(variable.Identifier.IdentifierName, out var symbol);
        if (symbol == null)
        {
            return;
        }

        if (symbol.Reference)
        {
            Builder.AddString("(*" + variable.Identifier.IdentifierName + ")");
        }
        else
        {
            Builder.AddString(variable.Identifier.IdentifierName);
        }

        if (symbol.SymbolType is PascalArrayType)
        {
            //解析数组类型，获取左边界列表
            List<int> leftBounds = new();
            PascalType curType = symbol.SymbolType;
            while (curType is not PascalBasicType)
            {
                leftBounds.Add(curType.Convert<PascalArrayType>().Begin);
                curType = curType.Convert<PascalArrayType>().ElementType;
            }

            //将数组维度信息向下传递
            variable.VarPart.LeftBounds.AddRange(leftBounds);
        }
    }

    public override void PreVisit(IdentifierVarPart identifierVarPart)
    {
        base.PreVisit(identifierVarPart);
        identifierVarPart.OnIndexGenerator += (_, e) =>
        {
            e.IndexParameters.IsIndex = true;
            e.IndexParameters.LeftBounds = identifierVarPart.LeftBounds;
        };
    }

    public override void PreVisit(ExpressionList expressionList)
    {
        base.PreVisit(expressionList);

        if (expressionList.IsIndex)
        {
            expressionList.Expression.LeftBound = expressionList.LeftBounds.Last();
            expressionList.Expression.IsIndex = true;
        }

        if (expressionList.IsParamList)
        {
            expressionList.Expression.ReferenceParam = expressionList.ParameterTypes.Last().IsVar;
            expressionList.Expression.IsParam = true;
        }
        expressionList.OnExpressionList += (_, e) =>
        {
            if (expressionList.IsIndex)
            {
                e.ExpressionList.IsIndex = true;
                expressionList.LeftBounds.RemoveAt(expressionList.LeftBounds.Count - 1);
                e.ExpressionList.LeftBounds = expressionList.LeftBounds;
            }

            if (expressionList.IsParamList)
            {
                e.ExpressionList.IsParamList = true;
                for (int i = 0; i < expressionList.ParameterTypes.Count - 1; i++)
                {
                    e.ExpressionList.ParameterTypes.Add(expressionList.ParameterTypes[i]);
                }
            }
        };
    }

    public override void PreVisit(Expression expression)
    {
        base.PreVisit(expression);
        if (expression.IsIndex)
        {
            Builder.AddString("[");
        }

        if (expression.IsForConditionBegin)
        {
            Builder.AddString("for(" + expression.Iterator.IdentifierName + " = ");
        }
        if (expression.IsForConditionEnd)
        {
            Builder.AddString(expression.Iterator.IdentifierName + " <= ");
        }
        if (expression.IsAssign)
        {
            Builder.AddString(" = ");
        }

        if (expression.ReferenceParam)
        {
            Builder.AddString("&");
        }
    }

    public override void PostVisit(Expression expression)
    {
        base.PostVisit(expression);
        if (expression.IsIndex)
        {
            //数组下标减去当前维度的左边界
            Builder.AddString("-" + expression.LeftBound + "]");
        }
        if (expression.IsForConditionEnd)
        {
            Builder.AddString("; " + expression.Iterator.IdentifierName + "++)");
        }
        if (expression is { IsParam: true, LastParam: false })
        {
            Builder.AddString(", ");
        }
    }

    public override void PostVisit(Factor factor)
    {
        base.PostVisit(factor);
        factor.OnNumberGenerator += (_, e) =>
        {
            var token = e.Token;
            string num = token.NumberType == NumberType.Integer ? token.ParseAsInteger().ToString() :
                token.ParseAsReal().ToString(CultureInfo.InvariantCulture);
            Builder.AddString(num);
        };
        factor.OnNotGenerator += (_, _) =>
        {
            Builder.AddString(")");
        };
        factor.OnUminusGenerator += (_, _) =>
        {
            Builder.AddString(")");
        };
    }

    public override void PreVisit(Factor factor)
    {
        base.PreVisit(factor);

        factor.OnNotGenerator += (_, _) =>
        {
            Builder.AddString("(~");
        };
        factor.OnUminusGenerator += (_, _) =>
        {
            Builder.AddString("(-");
        };
    }

    public override void PostVisit(MultiplyOperator multiplyOperator)
    {
        base.PostVisit(multiplyOperator);
        if (multiplyOperator.OperatorToken.TokenType == SemanticTokenType.Operator)
        {
             var operatorType = multiplyOperator.OperatorToken.Convert<OperatorSemanticToken>().OperatorType;
             if (operatorType == OperatorType.Multiply)
             {
                 Builder.AddString(" * ");
             }
             else if (operatorType == OperatorType.Divide)
             {
                 //实数除法，需要将操作数强转为double
                 Builder.AddString(" /(double)");
             }
        }
        else
        {
            var keywordType = multiplyOperator.OperatorToken.Convert<KeywordSemanticToken>().KeywordType;
            switch (keywordType)
            {
                case KeywordType.And:
                    Builder.AddString(" && ");
                    break;
                case KeywordType.Mod:
                    Builder.AddString(" % ");
                    break;
                default:
                    Builder.AddString(" / ");
                    break;
            }
        }
    }

    public override void PostVisit(AddOperator addOperator)
    {
        base.PostVisit(addOperator);
        var token = addOperator.OperatorToken;
        if (token.TokenType == SemanticTokenType.Operator)
        {
            var operatorType = token.Convert<OperatorSemanticToken>().OperatorType;
            if (operatorType == OperatorType.Plus)
            {
                Builder.AddString(" + ");
            }
            else if (operatorType == OperatorType.Minus)
            {
                Builder.AddString(" - ");
            }
        }
        else
        {
            Builder.AddString(" || ");
        }
    }

    public override void PostVisit(RelationOperator relationOperator)
    {
        base.PostVisit(relationOperator);
        var operatorType = relationOperator.OperatorToken.Convert<OperatorSemanticToken>().OperatorType;
        switch (operatorType)
        {
            case OperatorType.Equal:
                Builder.AddString(" == ");
                break;
            case OperatorType.Greater:
                Builder.AddString(" > ");
                break;
            case OperatorType.Less:
                Builder.AddString(" < ");
                break;
            case OperatorType.GreaterEqual:
                Builder.AddString(" >= ");
                break;
            case OperatorType.LessEqual:
                Builder.AddString(" <= ");
                break;
            case OperatorType.NotEqual:
                Builder.AddString(" != ");
                break;
        }
    }

    public override void PostVisit(TerminatedSyntaxNode terminatedSyntaxNode)
    {
        base.PostVisit(terminatedSyntaxNode);
        string literalValue = terminatedSyntaxNode.Token.LiteralValue;
        switch (literalValue)
        {
            case "if":
                Builder.AddString("if(");
                break;
            case "then":
                Builder.AddString(")\n");
                break;
            case "else":
                Builder.AddString("else\n");
                break;
            case "to":
                Builder.AddString("; ");
                break;
        }
    }

    /// <summary>
    /// 尝试将pascalBasicType解析成C语言的基本类型
    /// </summary>
    /// <returns>C语言形式的基本类型名</returns>
    /// <exception cref="InvalidOperationException"></exception>
    private string TryParseBasicType(PascalType pascalType)
    {
        if (pascalType is PascalBasicType basicType)
        {
            if (basicType == PascalBasicType.Integer)
            {
                return "int";
            }
            if (basicType == PascalBasicType.Real)
            {
                return "double";
            }

            if (basicType == PascalBasicType.Character)
            {
                return "char";
            }

            if (basicType == PascalBasicType.Boolean)
            {
                return "bool";
            }

            if (basicType == PascalBasicType.Void)
            {
                return "void";
            }
        }

        throw new InvalidOperationException("Not a basic type");
    }

    /// <summary>
    /// 尝试解析Pascal数组类型
    /// </summary>
    /// <param name="pascalType"></param>
    /// <param name="basicTypeName">数组实际存储的元素类型</param>
    /// <param name="periods">数组下标定义</param>
    private void TryParseArrayType(PascalType pascalType, out string basicTypeName, out string periods)
    {
        periods = "";
        PascalType curType = pascalType;
        //依次处理数组每一维
        while (curType is PascalArrayType pascalArrayType)
        {
            int begin = pascalArrayType.Begin;
            int end = pascalArrayType.End;
            //C语言数组下标从0开始，所以下标要减去begin
            periods += "[" + (end - begin + 1) + "]";
            curType = pascalArrayType.ElementType;
        }

        basicTypeName = TryParseBasicType(curType);
    }
}
