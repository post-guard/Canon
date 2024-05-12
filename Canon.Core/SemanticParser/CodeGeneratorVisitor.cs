using System.Globalization;
using Canon.Core.CodeGenerators;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;
using Canon.Core.SyntaxNodes;
using BasicType = Canon.Core.Enums.BasicType;

namespace Canon.Core.SemanticParser;

public class CodeGeneratorVisitor : TypeCheckVisitor
{
    public CCodeBuilder Builder { get; } = new();

    public override void PreVisit(ProgramHead programHead)
    {
        base.PreVisit(programHead);

        Builder.AddLine("#include <stdio.h>");
        Builder.AddLine("#include <stdbool.h>");
    }

    public override void PreVisit(ProgramBody programBody)
    {
        base.PreVisit(programBody);

        programBody.CompoundStatement.IsMain = true;
    }

    public override void PostVisit(ConstDeclaration constDeclaration)
    {
        base.PreVisit(constDeclaration);

        (IdentifierSemanticToken token, ConstValue constValue) = constDeclaration.ConstValue;

        if (SymbolTable.TryGetSymbol(token.IdentifierName, out Symbol? symbol))
        {
            Builder.AddLine(
                $"const {GenerateBasicTypeString(symbol.SymbolType)} {token.IdentifierName} = {constValue.ValueString};");
        }
    }

    public override void PostVisit(ConstValue constValue)
    {
        base.PostVisit(constValue);

        constValue.OnNumberGenerator += (_, e) =>
        {
            string numberValue = e.IsNegative ? "-" : "+";

            if (constValue.ConstType == PascalBasicType.Integer)
            {
                numberValue += e.Token.ParseAsInteger().ToString();
            }
            else if (constValue.ConstType == PascalBasicType.Real)
            {
                numberValue += e.Token.ParseAsReal().ToString(CultureInfo.CurrentCulture);
            }

            constValue.ValueString = numberValue;
        };

        constValue.OnCharacterGenerator += (_, e) => { constValue.ValueString = $"'{e.Token.LiteralValue}'"; };
    }

    public override void PreVisit(VarDeclaration varDeclaration)
    {
        base.PreVisit(varDeclaration);

        varDeclaration.IdentifierList.IsVariableDefinition = true;
    }

    public override void PostVisit(VarDeclaration varDeclaration)
    {
        base.PostVisit(varDeclaration);

        if (varDeclaration.IdentifierList.DefinitionType is PascalBasicType)
        {
            Builder.AddLine($"{GenerateBasicTypeString(varDeclaration.IdentifierList.DefinitionType)} " +
                            $"{varDeclaration.Token.IdentifierName};");
        }

        if (varDeclaration.IdentifierList.DefinitionType is PascalArrayType)
        {
            (string basicValue, string periodValue) = GenerateArrayTypeString(
                varDeclaration.IdentifierList.DefinitionType);

            Builder.AddLine($"{basicValue} {varDeclaration.Token.IdentifierName}{periodValue};");
        }
    }

    public override void PreVisit(IdentifierList identifierList)
    {
        base.PreVisit(identifierList);

        identifierList.OnIdentifierGenerator += (_, e) =>
        {
            if (identifierList.IsVariableDefinition)
            {
                e.IdentifierList.IsVariableDefinition = true;
            }
        };
    }

    public override void PostVisit(IdentifierList identifierList)
    {
        base.PostVisit(identifierList);

        identifierList.OnIdentifierGenerator += (_, e) =>
        {
            if (!identifierList.IsVariableDefinition)
            {
                return;
            }

            if (identifierList.DefinitionType is PascalArrayType)
            {
                (string basicTypeString, string periodString) = GenerateArrayTypeString(identifierList.DefinitionType);

                Builder.AddLine($"{basicTypeString} {e.IdentifierToken.IdentifierName}{periodString};");
            }

            if (identifierList.DefinitionType is PascalBasicType)
            {
                Builder.AddLine($"{GenerateBasicTypeString(identifierList.DefinitionType)} " +
                                $"{e.IdentifierToken.IdentifierName};");
            }
        };
    }

    public override void PreVisit(CompoundStatement compoundStatement)
    {
        if (compoundStatement.IsMain)
        {
            Builder.AddLine("int main()");
        }

        Builder.BeginScope();
    }

    public override void PostVisit(CompoundStatement compoundStatement)
    {
        if (compoundStatement.IsMain)
        {
            Builder.AddLine("return 0;");
        }

        Builder.EndScope();
    }

    public override void PreVisit(Factor factor)
    {
        base.PreVisit(factor);

        factor.OnParethnesisGenerator += (_, e) => { e.Expression.IsCondition = factor.IsCondition; };

        factor.OnNotGenerator += (_, e) => { e.Factor.IsCondition = factor.IsCondition; };

        factor.OnUminusGenerator += (_, e) => { e.Factor.IsCondition = factor.IsCondition; };

        factor.OnPlusGenerator += (_, e) => { e.Factor.IsCondition = factor.IsCondition; };
    }

    public override void PostVisit(Factor factor)
    {
        base.PostVisit(factor);

        factor.OnNumberGenerator += (_, e) =>
        {
            string temporaryName = GenerateTemporaryVariable();

            switch (e.Token.NumberType)
            {
                case NumberType.Integer:
                    Builder.AddLine($"int {temporaryName} = {e.Token.ParseAsInteger()};");
                    break;
                case NumberType.Real:
                    Builder.AddLine($"double {temporaryName} = {e.Token.ParseAsReal()};");
                    break;
            }

            factor.VariableName = temporaryName;
        };

        factor.OnBooleanGenerator += (_, e) =>
        {
            string temporaryName = GenerateTemporaryVariable();
            string value = e.Value ? "true" : "false";

            Builder.AddLine($"bool {temporaryName} = {value};");
            factor.VariableName = temporaryName;
        };

        factor.OnVariableGenerator += (_, e) =>
        {
            if (SymbolTable.TryGetSymbol(e.Variable.VariableName, out Symbol? symbol))
            {
                // 处理不带括号调用无参函数的问题
                if (symbol.SymbolType is PascalFunctionType { Parameters.Count: 0 } functionType)
                {
                    string temporaryName = GenerateTemporaryVariable();

                    Builder.AddLine($"{GenerateBasicTypeString(functionType.ReturnType)} {temporaryName} = " +
                                    $"{symbol.SymbolName}_pascal_procedure();");
                    factor.VariableName = temporaryName;
                    return;
                }
            }

            factor.VariableName = e.Variable.VariableName;
        };

        factor.OnParethnesisGenerator += (_, e) => { factor.VariableName = e.Expression.VariableName; };

        factor.OnNotGenerator += (_, e) =>
        {
            string temporaryName = GenerateTemporaryVariable();

            Builder.AddLine(
                $"{GenerateBasicTypeString(factor.VariableType)} {temporaryName} = ~{e.Factor.VariableName};");
            factor.VariableName = temporaryName;
        };

        factor.OnPlusGenerator += (_, e) => { factor.VariableName = e.Factor.VariableName; };

        factor.OnUminusGenerator += (_, e) =>
        {
            string temporaryName = GenerateTemporaryVariable();

            Builder.AddLine(
                $"{GenerateBasicTypeString(factor.VariableType)} {temporaryName} = -{e.Factor.VariableName};");
            factor.VariableName = temporaryName;
        };

        factor.OnProcedureCallGenerator += (_, e) =>
        {
            string temporaryName = GenerateTemporaryVariable();

            Builder.AddLine($"{GenerateBasicTypeString(factor.VariableType)} {temporaryName}= " +
                            $"{GenerateProcedureCall(e.ProcedureName.IdentifierName, e.Parameters)};");
            factor.VariableName = temporaryName;
        };
    }

    public override void PostVisit(Variable variable)
    {
        base.PostVisit(variable);

        if (!SymbolTable.TryGetSymbol(variable.Identifier.IdentifierName, out Symbol? symbol))
        {
            return;
        }

        PascalType type = symbol.SymbolType;

        if (type is PascalArrayType)
        {
            string indexValue = string.Empty;
            foreach (Expression expression in variable.VarPart.Expressions)
            {
                PascalArrayType arrayType = type.Convert<PascalArrayType>();
                string temporaryName = GenerateTemporaryVariable();

                Builder.AddLine($"int {temporaryName} = {expression.VariableName} - {arrayType.Begin};");

                indexValue += $"[{temporaryName}]";
                type = arrayType.ElementType;
            }

            variable.VariableName = variable.Identifier.IdentifierName + indexValue;
            return;
        }

        // 虽然这里这样可能会生成来类似于 &*x 的代码
        // 但是可以正常运行
        variable.VariableName =
            symbol.SymbolType.IsReference
                ? $"*{variable.Identifier.IdentifierName}"
                : variable.Identifier.IdentifierName;
    }

    public override void PreVisit(Term term)
    {
        base.PreVisit(term);

        term.OnMultiplyGenerator += (_, e) =>
        {
            e.Left.IsCondition = term.IsCondition;
            e.Right.IsCondition = term.IsCondition;
        };

        term.OnFactorGenerator += (_, e) => { e.Factor.IsCondition = term.IsCondition; };
    }

    public override void PostVisit(Term term)
    {
        base.PostVisit(term);

        term.OnFactorGenerator += (_, e) => { term.VariableName = e.Factor.VariableName; };

        term.OnMultiplyGenerator += (_, e) =>
        {
            string temporaryName = GenerateTemporaryVariable();

            Builder.AddLine(
                $"{GenerateBasicTypeString(term.VariableType)} {temporaryName} = " +
                $"{e.Left.VariableName} {GenerateMultipleOperator(e.Operator)} {e.Right.VariableName};");
            term.VariableName = temporaryName;
        };
    }

    public override void PreVisit(SimpleExpression simpleExpression)
    {
        base.PreVisit(simpleExpression);

        simpleExpression.OnAddGenerator += (_, e) =>
        {
            e.Left.IsCondition = simpleExpression.IsCondition;
            e.Right.IsCondition = simpleExpression.IsCondition;
        };

        simpleExpression.OnTermGenerator += (_, e) => { e.Term.IsCondition = simpleExpression.IsCondition; };
    }

    public override void PostVisit(SimpleExpression simpleExpression)
    {
        base.PostVisit(simpleExpression);

        simpleExpression.OnTermGenerator += (_, e) => { simpleExpression.VariableName = e.Term.VariableName; };

        simpleExpression.OnAddGenerator += (_, e) =>
        {
            string temporaryName = GenerateTemporaryVariable();

            Builder.AddLine(
                $"{GenerateBasicTypeString(simpleExpression.VariableType)} {temporaryName} = " +
                $"{e.Left.VariableName} {GenerateAddOperator(e.Operator)} {e.Right.VariableName};");

            simpleExpression.VariableName = temporaryName;
        };
    }

    public override void PreVisit(Expression expression)
    {
        base.PreVisit(expression);

        expression.OnSimpleExpressionGenerator += (_, e) =>
        {
            e.SimpleExpression.IsCondition = expression.IsCondition;
        };

        expression.OnRelationGenerator += (_, e) =>
        {
            e.Left.IsCondition = expression.IsCondition;
            e.Right.IsCondition = expression.IsCondition;
        };
        if (expression.IsWhileCondition)
        {
            GenerateWhileLabel();
            Builder.AddLine($"""
                             {_whileBeginLabels.Peek()}:;
                             """);
        }
    }

    public override void PostVisit(Expression expression)
    {
        base.PostVisit(expression);

        expression.OnSimpleExpressionGenerator += (_, e) =>
        {
            expression.VariableName = e.SimpleExpression.VariableName;

            HandleExpression(expression);
        };

        expression.OnRelationGenerator += (_, e) =>
        {
            string temporaryName = GenerateTemporaryVariable();

            Builder.AddLine($"{GenerateBasicTypeString(expression.VariableType)} {temporaryName} = " +
                            $"{e.Left.VariableName} {GenerateRelationOperator(e.Operator)} {e.Right.VariableName};");
            expression.VariableName = temporaryName;

            HandleExpression(expression);
        };
    }

    private void HandleExpression(Expression expression)
    {
        if (expression.IsIfCondition)
        {
            _ifConditionNames.Push(expression.VariableName);
        }

        if (expression.IsForConditionBegin)
        {
            _forBeginConditions.Push(expression.VariableName);
        }

        if (expression.IsForConditionEnd)
        {
            _forEndConditions.Push(expression.VariableName);
        }

        if (expression.IsWhileCondition)
        {
            _whileConditionNames.Push(expression.VariableName);
        }
    }


    /// <summary>
    /// 存储IF语句中条件变量的名称
    /// </summary>
    private readonly Stack<string> _ifConditionNames = new();

    /// <summary>
    /// IF语句中成功分支的标签
    /// </summary>
    private readonly Stack<string> _ifTrueLabels = new();

    /// <summary>
    /// IF语句中失败分支的标签
    /// </summary>
    private readonly Stack<string> _ifFalseLabels = new();

    /// <summary>
    /// IF语句中结束的标签
    /// </summary>
    private readonly Stack<string> _ifEndLabels = new();

    /// <summary>
    /// FOR语句中的循环变量名称
    /// </summary>
    private readonly Stack<string> _forVariables = new();

    /// <summary>
    /// FOR语句中的循环变量的初始值
    /// </summary>
    private readonly Stack<string> _forBeginConditions = new();

    /// <summary>
    /// FOR语句中循环变量的判断值
    /// </summary>
    private readonly Stack<string> _forEndConditions = new();

    /// <summary>
    /// FOR语句开始的标签
    /// </summary>
    private readonly Stack<string> _forLabels = new();

    /// <summary>
    /// FOR语句条件判断部分的标签
    /// </summary>
    private readonly Stack<string> _forConditionLabels = new();

    /// <summary>
    /// FOR语句结束的标签
    /// </summary>
    private readonly Stack<string> _forEndLabels = new();

    /// <summary>
    /// WHILE语句条件变量的标签
    /// </summary>
    private readonly Stack<string> _whileConditionNames = new();

    /// <summary>
    /// WHILE语句开始的标签
    /// </summary>
    private readonly Stack<string> _whileBeginLabels = new();

    /// <summary>
    /// WHILE语句结束的标签
    /// </summary>
    private readonly Stack<string> _whileEndLabels = new();

    public override void PreVisit(Statement statement)
    {
        base.PreVisit(statement);

        statement.OnIfGenerator += (_, e) =>
        {
            e.Condition.IsIfCondition = true;
            e.Condition.IsCondition = true;
        };

        statement.OnForGenerator += (_, e) =>
        {
            e.Begin.IsForConditionBegin = true;
            e.End.IsForConditionEnd = true;
            e.Do.IsForNode = true;
            _forVariables.Push(e.Iterator.IdentifierName);
        };

        statement.OnWhileGenerator += (_, e) =>
        {
            e.Do.IsWhileNode = true;
            e.Condition.IsWhileCondition = true;
        };
    }

    public override void PostVisit(Statement statement)
    {
        base.PostVisit(statement);

        statement.OnAssignGenerator += (_, e) =>
        {
            Builder.AddLine($"{e.Variable.VariableName} = {e.Expression.VariableName};");
        };

        statement.OnForGenerator += (_, _) =>
        {
            Builder.AddLine($"""
                             {_forVariables.Peek()} = {_forVariables.Peek()} + 1;
                             goto {_forConditionLabels.Peek()};
                             {_forEndLabels.Peek()}:;
                             """);

            _forLabels.Pop();
            _forConditionLabels.Pop();
            _forEndLabels.Pop();
            _forVariables.Pop();
            _forBeginConditions.Pop();
            _forEndConditions.Pop();
        };

        statement.OnWhileGenerator += (_, _) =>
        {
            Builder.AddLine($"""
                             goto {_whileBeginLabels.Peek()};
                             {_whileEndLabels.Peek()}:;
                             """);
            _whileBeginLabels.Pop();
            _whileEndLabels.Pop();
            _whileConditionNames.Pop();
        };
    }

    public override void PreVisit(ElsePart elsePart)
    {
        base.PreVisit(elsePart);

        // 这是成功分支跳过Else的语句
        Builder.AddLine($"goto {_ifEndLabels.Peek()};");
        Builder.AddLine($"{_ifFalseLabels.Peek()}:;");
    }

    public override void PostVisit(ElsePart elsePart)
    {
        base.PostVisit(elsePart);

        Builder.AddLine($"{_ifEndLabels.Peek()}:;");
        _ifConditionNames.Pop();
        _ifTrueLabels.Pop();
        _ifFalseLabels.Pop();
        _ifEndLabels.Pop();
    }

    public override void PostVisit(ProcedureCall procedureCall)
    {
        base.PostVisit(procedureCall);

        Builder.AddLine(GenerateProcedureCall(procedureCall.ProcedureId.IdentifierName, procedureCall.Parameters) +
                        ";");
    }

    public override void PostVisit(SubprogramHead subprogramHead)
    {
        base.PostVisit(subprogramHead);

        subprogramHead.OnProcedureGenerator += (_, _) =>
        {
            GenerateProcedureHead(subprogramHead.SubprogramName.IdentifierName);
        };

        subprogramHead.OnFunctionGenerator += (_, _) =>
        {
            GenerateProcedureHead(subprogramHead.SubprogramName.IdentifierName);
        };
    }

    private Symbol? _function;

    private void GenerateProcedureHead(string procedureId)
    {
        if (!SymbolTable.TryGetParent(out SymbolTable? parent))
        {
            return;
        }

        if (!parent.TryGetSymbol(procedureId, out Symbol? symbol))
        {
            return;
        }

        if (symbol.SymbolType is not PascalFunctionType functionType)
        {
            return;
        }

        _function = symbol;
        string result =
            $"{GenerateBasicTypeString(functionType.ReturnType)} {procedureId}_pascal_procedure(";

        // 控制格式
        bool isStart = true;

        foreach (PascalParameterType parameter in functionType.Parameters)
        {
            string value = string.Empty;

            if (parameter.ParameterType is PascalBasicType)
            {
                value = $"{GenerateBasicTypeString(parameter.ParameterType)} {parameter.ParameterName}";
            }

            if (parameter.ParameterType is PascalArrayType)
            {
                value = $"{GenerateArrayTypeString(parameter.ParameterType)} {parameter.ParameterName}";
            }

            if (isStart)
            {
                result += value;
            }
            else
            {
                result += ", " + value;
            }

            isStart = false;
        }

        result += ")";

        Builder.AddLine(result);
    }

    public override void PreVisit(SubprogramBody subprogramBody)
    {
        base.PreVisit(subprogramBody);

        Builder.BeginScope();

        if (_function is null)
        {
            return;
        }

        PascalFunctionType functionType = _function.SymbolType.Convert<PascalFunctionType>();

        if (functionType.ReturnType != PascalBasicType.Void)
        {
            Builder.AddLine($"{GenerateBasicTypeString(functionType.ReturnType)} {_function.SymbolName};");
        }
    }

    public override void PostVisit(SubprogramBody subprogramBody)
    {
        base.PostVisit(subprogramBody);

        if (_function is null)
        {
            return;
        }

        PascalFunctionType functionType = _function.SymbolType.Convert<PascalFunctionType>();

        if (functionType.ReturnType != PascalBasicType.Void)
        {
            Builder.AddLine($"return {_function.SymbolName};");
        }

        Builder.EndScope();
    }

    public override void PostVisit(TerminatedSyntaxNode terminatedSyntaxNode)
    {
        base.PostVisit(terminatedSyntaxNode);

        if (terminatedSyntaxNode.Token.TokenType == SemanticTokenType.Keyword)
        {
            KeywordType keywordType = terminatedSyntaxNode.Token.Convert<KeywordSemanticToken>().KeywordType;

            switch (keywordType)
            {
                case KeywordType.Then:
                    GenerateIfLabel();

                    Builder.AddLine($"""
                                     if ({_ifConditionNames.Peek()})
                                         goto {_ifTrueLabels.Peek()};
                                     else
                                         goto {_ifFalseLabels.Peek()};
                                     {_ifTrueLabels.Peek()}:;
                                     """);
                    break;
                case KeywordType.To:
                    GenerateForLabel();

                    Builder.AddLine($"""
                                     {_forVariables.Peek()} = {_forBeginConditions.Peek()};
                                     {_forConditionLabels.Peek()}:;
                                     """);
                    break;
                case KeywordType.Do:
                    if (terminatedSyntaxNode.IsForNode)
                    {
                        Builder.AddLine($"""
                                         if ({_forVariables.Peek()} <= {_forEndConditions.Peek()})
                                             goto {_forLabels.Peek()};
                                         else
                                             goto {_forEndLabels.Peek()};
                                         {_forLabels.Peek()}:;
                                         """);
                    }

                    if (terminatedSyntaxNode.IsWhileNode)
                    {
                        // GenerateWhileLabel();
                        Builder.AddLine($"""
                                         if (!{_whileConditionNames.Peek()})
                                             goto {_whileEndLabels.Peek()};
                                         """);
                    }

                    break;
            }
        }
    }

    private string GenerateProcedureCall(string procedureId, List<Expression> parameters)
    {
        string isReturn = procedureId == "writeln" || procedureId == "readln" ? "\\n" : string.Empty;

        if (procedureId == "write" || procedureId == "writeln")
        {
            string result = $"printf(\"{GenerateFormatString(parameters, true) + isReturn}\"";

            foreach (Expression parameter in parameters)
            {
                result += $", {parameter.VariableName}";
            }

            return result + ")";
        }

        if (procedureId == "read" || procedureId == "readln")
        {
            string result = $"scanf(\"{GenerateFormatString(parameters) + isReturn}\"";

            foreach (Expression parameter in parameters)
            {
                result += $", &{parameter.VariableName}";
            }

            return result + ")";
        }

        if (!SymbolTable.TryGetSymbol(procedureId, out Symbol? symbol))
        {
            return string.Empty;
        }

        string parameterValue = string.Empty;

        PascalFunctionType functionType;
        if (symbol.SymbolType is not PascalFunctionType innerType)
        {
            // 处理函数内部的递归调用
            if (!SymbolTable.TryGetParent(out SymbolTable? parent))
            {
                return string.Empty;
            }

            if (!parent.TryGetSymbol(procedureId, out symbol))
            {
                return string.Empty;
            }

            if (symbol.SymbolType is PascalFunctionType outerType)
            {
                functionType = outerType;
            }
            else
            {
                return string.Empty;
            }
        }
        else
        {
            functionType = innerType;
        }

        foreach ((Expression parameter, PascalParameterType parameterType) in
                 parameters.Zip(functionType.Parameters))
        {
            if (parameterType.ParameterType.IsReference)
            {
                parameterValue += $", &{parameter.VariableName}";
            }
            else
            {
                parameterValue += $", {parameter.VariableName}";
            }
        }

        parameterValue = parameterValue == string.Empty ? parameterValue : parameterValue[1..];
        return $"{procedureId}_pascal_procedure({parameterValue})";
    }

    private static string GenerateFormatString(List<Expression> expressions, bool output = false)
    {
        string value = string.Empty;

        foreach (Expression expression in expressions)
        {
            if (expression.VariableType == PascalBasicType.Integer)
            {
                value += "%d";
            }

            if (expression.VariableType == PascalBasicType.Real)
            {
                // 这里需要按照输出调整
                // 在输出real的前面需要添加一个空格
                value += output ? "%.6lf" : "%lf";
            }

            if (expression.VariableType == PascalBasicType.Character)
            {
                value += "%c";
            }
        }

        return value;
    }

    private static string GenerateBasicTypeString(PascalType pascalType)
    {
        if (pascalType is not PascalBasicType basicType)
        {
            return string.Empty;
        }

        switch (basicType.Type)
        {
            case BasicType.Integer:
                return basicType.IsReference ? "int *" : "int";
            case BasicType.Real:
                return basicType.IsReference ? "double *" : "double";
            case BasicType.Character:
                return basicType.IsReference ? "char *" : "char";
            case BasicType.Boolean:
                return basicType.IsReference ? "bool *" : "bool";
        }

        return string.Empty;
    }

    private static (string, string) GenerateArrayTypeString(PascalType pascalType)
    {
        string periodString = string.Empty;

        while (pascalType is PascalArrayType arrayType)
        {
            periodString += $"[{arrayType.End - arrayType.Begin + 1}]";

            pascalType = arrayType.ElementType;
        }

        return (GenerateBasicTypeString(pascalType), periodString);
    }

    private static string GenerateMultipleOperator(MultiplyOperator multiplyOperator)
    {
        if (multiplyOperator.OperatorToken.TokenType == SemanticTokenType.Operator)
        {
            OperatorType operatorType = multiplyOperator.OperatorToken.Convert<OperatorSemanticToken>().OperatorType;
            if (operatorType == OperatorType.Multiply)
            {
                return "*";
            }
            else if (operatorType == OperatorType.Divide)
            {
                //实数除法，需要将操作数强转为double
                return "/(double)";
            }
        }
        else
        {
            KeywordType keywordType = multiplyOperator.OperatorToken.Convert<KeywordSemanticToken>().KeywordType;
            switch (keywordType)
            {
                case KeywordType.And:
                    return "&&";
                case KeywordType.Mod:
                    return "%";
                case KeywordType.Divide:
                    return "/";
            }
        }

        return string.Empty;
    }

    private static string GenerateAddOperator(AddOperator addOperator)
    {
        SemanticToken token = addOperator.OperatorToken;
        if (token.TokenType == SemanticTokenType.Operator)
        {
            OperatorType operatorType = token.Convert<OperatorSemanticToken>().OperatorType;
            if (operatorType == OperatorType.Plus)
            {
                return "+";
            }
            else if (operatorType == OperatorType.Minus)
            {
                return "-";
            }
        }
        else
        {
            return "||";
        }

        return string.Empty;
    }

    private static string GenerateRelationOperator(RelationOperator relationOperator)
    {
        var operatorType = relationOperator.OperatorToken.Convert<OperatorSemanticToken>().OperatorType;
        switch (operatorType)
        {
            case OperatorType.Equal:
                return "==";
            case OperatorType.Greater:
                return ">";
            case OperatorType.Less:
                return "<";
            case OperatorType.GreaterEqual:
                return ">=";
            case OperatorType.LessEqual:
                return "<=";
            case OperatorType.NotEqual:
                return "!=";
        }

        return string.Empty;
    }

    private long _temporaryVariableCount;

    /// <summary>
    /// 产生一个全局唯一的临时变量名
    /// </summary>
    private string GenerateTemporaryVariable()
    {
        string name = $"__temp_{_temporaryVariableCount}";
        _temporaryVariableCount += 1;
        return name;
    }

    private long _labelCount;

    /// <summary>
    /// 产生一对IF语句中的标签
    /// </summary>
    private void GenerateIfLabel()
    {
        _ifTrueLabels.Push($"if_true_{_labelCount}");
        _ifFalseLabels.Push($"if_false_{_labelCount}");
        _ifEndLabels.Push($"_if_end_{_labelCount}");

        _labelCount += 1;
    }

    /// <summary>
    /// 产生FOR语句中的标签
    /// </summary>
    private void GenerateForLabel()
    {
        _forLabels.Push($"for_{_labelCount}");
        _forConditionLabels.Push($"for_condition_{_labelCount}");
        _forEndLabels.Push($"for_end_{_labelCount}");

        _labelCount += 1;
    }

    /// <summary>
    /// 产生WHILE语句中的标签
    /// </summary>
    private void GenerateWhileLabel()
    {
        _whileBeginLabels.Push($"while_{_labelCount}");
        _whileConditionNames.Push($"while_condition_{_labelCount}");
        _whileEndLabels.Push($"while_end_{_labelCount}");

        _labelCount += 1;
    }
}
