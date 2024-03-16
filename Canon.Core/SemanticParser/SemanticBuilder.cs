using Canon.Core.Enums;
using Canon.Core.Exceptions;
using Canon.Core.GrammarParser;
using Canon.Core.LexicalParser;

namespace Canon.Core.SemanticParser;

public class SemanticBuilder
{
    private SymbolTable _curSymbolTable; //当前符号表

    private Stack<SymbolTable> _stack;

    public SemanticBuilder()
    {
        _curSymbolTable = new SymbolTable();
        _stack = new Stack<SymbolTable>();
    }


    /// <summary>
    /// 执行语义分析
    /// </summary>
    /// <param name="root">语法树根节点</param>
    public void Build(SyntaxNode root)
    {
        //新建一个符号表，压入栈
        _stack.Push(_curSymbolTable);
        //开始递归调用，完成构建符号表和类型检查
        SolveProgramStruct(root);
    }


    private void SolveProgramStruct(SyntaxNode root)
    {
        SolveProgramHead(root.Children[0]);
        SolveProgramBody(root.Children[2]);
    }

    private void SolveProgramHead(SyntaxNode root)
    {
        //不做任何处理
    }

    private void SolveProgramBody(SyntaxNode root)
    {
        SolveConstDeclarations(root.Children[0]);
        SolveVarDeclarations(root.Children[1]);
        SolveSubprogramDeclarations(root.Children[2]);
        SolveCompoundStatement(root.Children[3]);
    }

    private void SolveConstDeclarations(SyntaxNode root)
    {
        if (root.Children.Count > 1)
        {
            SolveConstDeclaration(root.Children[1]);
        }

    }

    private void SolveConstDeclaration(SyntaxNode root)
    {
        int offset = 0;
        if (!root.Children[0].IsTerminated)
        {
            SolveConstDeclaration(root.Children[0]);
            offset = 2;
        }

        var idName = root.Children[offset].GetSemanticToken().LiteralValue;
        _curSymbolTable.AddEntry(idName, SolveConstValue(root.Children[offset+2]), true, false);
    }

    private IdentifierType SolveConstValue(SyntaxNode root)
    {
        if (root.Children.Count == 3)
        {
            return new BasicType(BasicIdType.Char);
        }
        else
        {
            var t = ((NumberSemanticToken)root.GetSemanticToken()).NumberType;
            if (t == NumberType.Real)
            {
                return new BasicType(BasicIdType.Real);
            }
            else
            {
                return new BasicType(BasicIdType.Int);
            }
        }
    }

    private void SolveVarDeclarations(SyntaxNode root)
    {
        if (root.Children.Count > 1)
        {
            SolveVarDeclaration(root.Children[1]);
        }
    }

    private void SolveVarDeclaration(SyntaxNode root)
    {
        int offset = 0;
        if (root.Children.Count > 3)
        {
            SolveVarDeclaration(root.Children[0]);
            offset = 2;
        }

        List<string> idList = new List<string>();
        SolveIdList(root.Children[offset], idList);   //获取待定义的标识符列表

        var type = SolveType(root.Children[offset + 2]);  //获取类型

        //将符号批量插入符号表
        foreach(var id in idList)
        {
            _curSymbolTable.AddEntry(id, type, false, false);
        }

    }

    private void SolveIdList(SyntaxNode root, List<string> idList)
    {
        if (root.IsTerminated)
        {
            var word = root.GetSemanticToken().LiteralValue;
            if (word != ",")
            {
                idList.Add(word);
            }
        }

        foreach (var child in root.Children)
        {
            SolveIdList(child, idList);
        }
    }

    private IdentifierType SolveType(SyntaxNode root)
    {
        string typeName = root.Children[0].Children[0].GetSemanticToken().LiteralValue;
        //基本类型或记录类型
        if (root.Children.Count == 1)
        {
            //逐层向外检查类型是否在类型表中
            var cur = _curSymbolTable;
            while (cur != null && !cur.TypesTable.Check(typeName))
            {
                cur = cur.PreTable;
            }

            if (cur != null)
            {
                return cur.TypesTable.GetTypeByName(typeName);
            }

            throw new SemanticException(typeName + " is an undefined type!");
        }


        //数组类型
        var limitList = new List<Limits>();
        SolvePeriod(root.Children[2], limitList);
        return new ArrayType(limitList.Count, limitList, SolveType(root.Children[5]));
    }

    private void SolvePeriod(SyntaxNode root, List<Limits> limitsList)
    {
        int offset = 0;

        if (!root.Children[0].IsTerminated)
        {
            SolvePeriod(root.Children[0], limitsList); //递归获取子节点的界限列表
            offset = 2;
        }

        //token1和2对应数组当前维度的上下界
        var token1 = (NumberSemanticToken)root.Children[offset].GetSemanticToken();
        var t1 = token1.NumberType;
        var token2 = (NumberSemanticToken)root.Children[offset + 3].GetSemanticToken();
        var t2 = token2.NumberType;

        //检查数组上下界定义是否合法
        if (t1 == NumberType.Integer && t2 == NumberType.Integer)
        {
            int lower = int.Parse(token1.LiteralValue);
            int upper = int.Parse(token2.LiteralValue);
            if (upper >= lower)
            {
                if (lower >= 0)
                {
                    limitsList.Add(new Limits((uint)lower, (uint)upper));
                    return;
                }

                throw new SemanticException("Array's bounds must be nonnegative!");
            }

            throw new SemanticException("Array's upper bound must greater than low bound!");
        }

        throw new SemanticException("Array's bounds must be integer!");
    }

    private void SolveSubprogramDeclarations(SyntaxNode root)
    {
        if (root.Children.Count > 1)
        {
            SolveSubprogramDeclarations(root.Children[0]);
            SolveSubprogram(root.Children[1]);
            //处理完一个子过程/函数，将对应符号表出栈，当前符号表更新为新栈顶符号表
            _stack.Pop();
            _curSymbolTable = _stack.Peek();
        }

    }

    private void SolveSubprogram(SyntaxNode root)
    {
        SolveSubprogramHead(root.Children[0]);
        SolveSubprogramBody(root.Children[2]);
    }

    private void SolveSubprogramHead(SyntaxNode root)
    {
        //获取参数列表信息
        List<Param> paramList = new List<Param>();
        SolveFormalParameter(root.Children[2], paramList);

        //区分procedure和function
        IdentifierType identifierType;
        if (root.Children.Count == 3)
        {
            identifierType = new ProcType(paramList);

        }
        else
        {
            var type = SolveType(root.Children[4]); //获取函数返回值
            identifierType = new FuncType(paramList, type);
        }

        //创建子符号表
        SymbolTable subTable = new SymbolTable(_curSymbolTable);

        //将参数列表参数拷贝一份到子符号表
        foreach (var param in paramList)
        {
            subTable.AddEntry(param.Name, param.Type, false, param.IsVar);
        }

        //将proc/func头定义写入当前符号表
        _curSymbolTable.AddEntry(root.Children[1].GetSemanticToken().LiteralValue, identifierType, subTable);

        //子符号表入栈
        _stack.Push(subTable);
        //更新当前符号表为子符号表
        _curSymbolTable = subTable;
    }

    private void SolveFormalParameter(SyntaxNode root, List<Param> paramList)
    {
        if (root.Children.Count > 1)
        {
            SolveParameterList(root.Children[1], paramList);
        }
    }

    private void SolveParameterList(SyntaxNode root, List<Param> paramList)
    {
        int offset = 0;
        if (root.Children.Count > 1)
        {
            SolveParameterList(root, paramList);
            offset = 2;
        }

        SolveParameter(root.Children[offset], paramList);
    }

    private void SolveParameter(SyntaxNode root, List<Param> paramList)
    {
        bool isVarParam = false;
        SyntaxNode node = root.Children[0];
        if (node.GetNonTerminatorType() == NonTerminatorType.VarParameter)
        {
            isVarParam = true;
            node = node.Children[1];
        }

        List<string> list = new List<string>();
        SolveIdList(node.Children[0], list);
        var type = SolveType(node.Children[2]);

        foreach (var idName in list)
        {
            paramList.Add(new Param(idName, type, isVarParam));
        }
    }


    private void SolveSubprogramBody(SyntaxNode root)
    {
        SolveConstDeclarations(root.Children[0]);
        SolveVarDeclarations(root.Children[1]);
        SolveCompoundStatement(root.Children[2]);
    }

    private void SolveCompoundStatement(SyntaxNode root)
    {
        SolveStatementList(root.Children[1]);
    }

    private void SolveStatementList(SyntaxNode root)
    {
        int offset = 0;
        if (root.Children.Count > 1)
        {
            SolveStatementList(root.Children[0]);
            offset = 2;
        }

        SolveStatement(root.Children[offset]);
    }

    private void SolveStatement(SyntaxNode root)
    {
        var node = root.Children[0];
        if (node.IsTerminated)
        {
            //通过子节点个数判断用的statement的哪个产生式
            int childCount = root.Children.Count;
            switch (childCount)
            {
                case 3:
                    CheckFuncAssign(root);
                    break;
                case 5:
                    SolveIf(root);
                    break;
                case 8:
                    CheckForLoop(root);
                    break;
                default:
                    throw new SemanticException("Semantic analysis failed and an illegal node was detected");
            }
        }
        else
        {
            var nonTerminatorType = node.GetNonTerminatorType();
            switch (nonTerminatorType)
            {
                case NonTerminatorType.Variable:
                    SolveVariableAssign(root);
                    break;
                case NonTerminatorType.ProcedureCall:
                    SolveCall(node, false);
                    break;
                case NonTerminatorType.CompoundStatement:
                    SolveCompoundStatement(node);
                    break;
                default:
                    throw new SemanticException("Semantic analysis failed and an illegal node was detected");
            }
        }
    }

    /// <summary>
    /// 处理表达式
    /// </summary>
    /// <param name="root"></param>
    /// <returns>表达式类型</returns>
    private IdentifierType SolveExpression(SyntaxNode root)
    {
        if (root.Children.Count == 1)
        {
            return SolveSimpleExpression(root.Children[0]);
        }

        var type1 = SolveSimpleExpression(root.Children[0]);
        var type2 = SolveSimpleExpression(root.Children[2]);

        return CheckRelationOperation(type1, type2);
    }

    private IdentifierType SolveSimpleExpression(SyntaxNode root)
    {
        if (root.Children.Count == 1)
        {
            return SolveTerm(root.Children[0]);
        }

        var type1 = SolveSimpleExpression(root.Children[0]);
        var type2 = SolveTerm(root.Children[2]);

        return CheckAddOperation(type1, type2, root.Children[1].Children[0].GetSemanticToken());
    }

    private IdentifierType SolveTerm(SyntaxNode root)
    {
        if (root.Children.Count == 1)
        {
            return SolveFactor(root.Children[0]);
        }

        var type1 = SolveTerm(root.Children[0]);
        var type2 = SolveFactor(root.Children[2]);

        return CheckMultiplyOperation(type1, type2, root.Children[1].Children[0].GetSemanticToken());
    }

    private IdentifierType SolveFactor(SyntaxNode root)
    {
        if (root.Children.Count == 1)
        {
            if (root.Children[0].IsTerminated)
            {
                var numberSemanticToken = (NumberSemanticToken)root.Children[0].GetSemanticToken();
                if (numberSemanticToken.NumberType == NumberType.Real)
                {
                    return _curSymbolTable.TypesTable.GetTypeByName("real");
                }

                return _curSymbolTable.TypesTable.GetTypeByName("integer");
            }

            return SolveVariable(root.Children[0]);
        }

        //处理 Factor -> (expression)
        if (root.Children.Count == 3)
        {
            return SolveExpression(root.Children[1]);
        }

        //函数掉用
        if (root.Children.Count == 4)
        {
            return SolveCall(root, true);
        }

        //处理 Factor -> not Factor
        if (root.Children[0].GetSemanticToken() == new Terminator(KeywordType.Not))
        {
            var type = SolveFactor(root.Children[1]);
            if (type == _curSymbolTable.TypesTable.GetTypeByName("boolean"))
            {
                return type;
            }

            throw new SemanticException("NOT can only be used for Boolean expressions");
        }

        //处理数字取负
        var type1 = SolveFactor(root.Children[1]);
        if (type1 == _curSymbolTable.TypesTable.GetTypeByName("integer") ||
            type1 == _curSymbolTable.TypesTable.GetTypeByName("real"))
        {
            return type1;
        }

        throw new SemanticException("minus can only be used on integer or real");
    }

    private void SolveIf(SyntaxNode root)
    {
        var boolType = _curSymbolTable.TypesTable.GetTypeByName("boolean");
        if (SolveExpression(root.Children[0]) != boolType)
        {
            throw new SemanticException("The conditional expression of the if statement must be of type Boolean");
        }

        SolveStatement(root.Children[3]);
        SolveElsePart(root.Children[4]);
    }

    private void SolveElsePart(SyntaxNode root)
    {
        if (root.Children.Count > 1)
        {
            SolveStatement(root.Children[1]);
        }
    }

    private void CheckForLoop(SyntaxNode root)
    {
        var intType = _curSymbolTable.TypesTable.GetTypeByName("integer");
        string idName = root.Children[1].GetSemanticToken().LiteralValue;
        if (_curSymbolTable.Find(idName) && _curSymbolTable.GetIdTypeByName(idName) == intType)
        {
            if (SolveExpression(root.Children[3]) == intType && SolveExpression(root.Children[5]) == intType)
            {
                SolveStatement(root.Children[7]);
            }
            else
            {
                throw new SemanticException("The upper and lower bounds of the loop variable in the for loop" +
                                            " must be set to integer");
            }
        }
        else
        {
            throw new SemanticException("The loop variable in the for loop must be integer");
        }
    }

    private void SolveVariableAssign(SyntaxNode root)
    {
        var varType = SolveVariable(root.Children[0]);
        var expType = SolveExpression(root.Children[2]);
        CheckAssign(varType, expType);

        //常量不允许被重复赋值
        var idName = root.Children[0].GetSemanticToken().LiteralValue;
        if (_curSymbolTable.IsConst(idName))
        {
            throw new SemanticException("Constants defined as const are not allowed to be assigned repeatedly");
        }
    }

    private IdentifierType SolveVariable(SyntaxNode root)
    {
        var idName = root.Children[0].GetSemanticToken().LiteralValue;
        var idType = _curSymbolTable.GetIdTypeByName(idName);
        if (idType is BasicType)
        {
            if (root.Children[1].Children.Count == 1)
            {
                return idType;
            }

            throw new SemanticException("The reference to variable "+ idName+  " is illegal");
        }

        //暂时只考虑数组类型
        List<IdentifierType> typeList = new List<IdentifierType>();
        SolveExpressionList(root.Children[1].Children[1], typeList);

        int dimension = ((ArrayType)idType).Dimension;
        //数组引用维数一致
        if (typeList.Count == dimension)
        {
            //每个维度的表达式类型都是int
            var IntType = _curSymbolTable.TypesTable.GetTypeByName("integer");
            foreach (var t in typeList)
            {
                if (t != IntType)
                {
                    throw new SemanticException("Array's index must be integer!");
                }
            }

            return ((ArrayType)idType).ElementType;
        }

        throw new SemanticException("The reference to array " + idName + " requires " + dimension + " subscripts");
    }

    private void SolveExpressionList(SyntaxNode root, List<IdentifierType> typeList)
    {
        int offset = 0;
        if (root.Children.Count > 1)
        {
            SolveExpressionList(root.Children[0], typeList);
            offset = 2;
        }

        typeList.Add(SolveExpression(root.Children[offset]));
    }

    private IdentifierType SolveCall(SyntaxNode root, bool isFunc)
    {
        var idName = root.Children[0].GetSemanticToken().LiteralValue;
        IdentifierType idType = _curSymbolTable.GetIdTypeByName(idName);
        //获取调用表达式的类型列表
        List<IdentifierType> typeList = new List<IdentifierType>();
        SolveExpressionList(root.Children[2], typeList);
        //将调用类型列表和定义参数列表的类型一一比对
        var paramList = isFunc ? ((FuncType)idType).ParamTypeList : ((ProcType)idType).ParamTypeList;
        if (paramList.Count == typeList.Count)
        {
            int n = paramList.Count;
            for (int i = 0; i < n; i++) {
                if (paramList[i].Type != typeList[i])
                {
                    throw new SemanticException("The parameter types are inconsistent");
                }
            }

            return isFunc ? ((FuncType)idType).ReturnType : new NonType();
        }

        throw new SemanticException("The number of parameters of procedure is inconsistent");
    }

    private void CheckFuncAssign(SyntaxNode root)
    {
        if (_curSymbolTable.PreTable is null)
        {
            throw new SemanticException("Not allowed to assign a value to a function name in the main program");
        }
        //获取函数返回值类型
        var idType =
            ((FuncType)_curSymbolTable.PreTable.GetIdTypeByName(root.Children[0].GetSemanticToken().LiteralValue)).ReturnType;
        //获取右侧表达式类型
        var expressionType = SolveExpression(root.Children[2]);
        //对赋值进行类型检查
        CheckAssign(idType, expressionType);
    }

    /// <summary>
    /// 检查赋值语句左右部分类型是否相容
    /// </summary>
    /// <param name="leftType">赋值号左边类型(若为函数，则取返回值)</param>
    /// <param name="rightType">赋值号右边类型</param>
    private void CheckAssign(IdentifierType leftType, IdentifierType rightType)
    {
        if (leftType == rightType)
        {
            return;
        }

        var intType = _curSymbolTable.TypesTable.GetTypeByName("integer");
        var realType = _curSymbolTable.TypesTable.GetTypeByName("real");

        if (leftType == realType && rightType == intType)   //int可以赋值给real
        {
            return;
        }

        throw new SemanticException("Incompatible types in assign operation");
    }

    /// <summary>
    /// 检查关系操作(比大小)的操作数类型
    /// </summary>
    /// <param name="leftType">左边符号类型</param>
    /// <param name="rightType">右边符号类型</param>
    /// <returns>成功则返回boolean类型</returns>
    private IdentifierType CheckRelationOperation(IdentifierType leftType, IdentifierType rightType)
    {
        var intType = _curSymbolTable.TypesTable.GetTypeByName("integer");
        var realType = _curSymbolTable.TypesTable.GetTypeByName("real");
        var boolType = _curSymbolTable.TypesTable.GetTypeByName("boolean");

        //左右为相等的基本类型或 一个int和一个real
        if (leftType == rightType && leftType is BasicType ||
            leftType == intType && rightType == realType ||
            leftType == realType && rightType == intType)
        {
            return boolType;
        }

        throw new SemanticException("Incompatible types in relation operations");
    }


    private IdentifierType CheckAddOperation(IdentifierType leftType, IdentifierType rightType, SemanticToken semanticToken)
    {
        // or操作两边必为boolean
        var boolType = _curSymbolTable.TypesTable.GetTypeByName("boolean");
        if (semanticToken == new Terminator(KeywordType.Or))
        {
            if (leftType == boolType && rightType == boolType)
            {
                return boolType;
            }

            throw new SemanticException("Incompatible types in add operation \"or\"");
        }

        var intType = _curSymbolTable.TypesTable.GetTypeByName("integer");
        var realType = _curSymbolTable.TypesTable.GetTypeByName("real");

        //左右为相等的基本类型但不为boolean
        if (leftType == rightType && leftType is BasicType && leftType != boolType)
        {
            return leftType;
        }
        //int和real可兼容
        if (leftType == intType && rightType == realType || leftType == realType && rightType == intType)
        {
            return realType;
        }

        throw new SemanticException("Incompatible types in add operations");
    }

    private IdentifierType CheckMultiplyOperation(IdentifierType leftType, IdentifierType rightType, SemanticToken semanticToken)
    {
        // and操作两边必为boolean
        var boolType = _curSymbolTable.TypesTable.GetTypeByName("boolean");
        if (semanticToken == new Terminator(KeywordType.And))
        {
            if (leftType == boolType && rightType == boolType)
            {
                return boolType;
            }

            throw new SemanticException("Incompatible types in multiply operation \"and\"");
        }

        // div和mod操作数必为int
        var intType = _curSymbolTable.TypesTable.GetTypeByName("integer");
        if (semanticToken == new Terminator(KeywordType.Mod) || semanticToken == new Terminator(KeywordType.Divide))
        {
            if (leftType == intType && rightType == intType)
            {
                return intType;
            }
            throw new SemanticException("Incompatible types in multiply operation \"mod/div\"");
        }

        //都是int或都是real
        var realType = _curSymbolTable.TypesTable.GetTypeByName("real");
        if (leftType == intType && rightType == intType || leftType == realType && rightType == realType)
        {
            return leftType;
        }
        //一个是int，另一个real
        if (leftType == intType && rightType == realType || leftType == realType && rightType == intType)
        {
            return realType;
        }

        throw new SemanticException("Incompatible types in multiply operations");
    }
}
