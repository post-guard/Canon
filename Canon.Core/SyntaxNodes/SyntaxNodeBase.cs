using Canon.Core.Abstractions;
using Canon.Core.CodeGenerators;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Core.SyntaxNodes;

public abstract class SyntaxNodeBase : ICCodeGenerator
{
    public abstract bool IsTerminated { get; }

    public abstract void PreVisit(SyntaxNodeVisitor visitor);

    public abstract void PostVisit(SyntaxNodeVisitor visitor);

    public T Convert<T>() where T : SyntaxNodeBase
    {
        T? result = this as T;

        if (result is null)
        {
            throw new InvalidOperationException("Can't cast into target SyntaxNode");
        }

        return result;
    }

    /// <summary>
    /// 语法树节点基类对于生成C代码的默认实现
    /// </summary>
    public virtual void GenerateCCode(CCodeBuilder builder)
    {
    }

    public override string ToString()
    {
        return IsTerminated.ToString();
    }

    public static SyntaxNodeBase Create(SemanticToken token)
    {
        return new TerminatedSyntaxNode { Token = token };
    }

    public static SyntaxNodeBase Create(NonTerminatorType type, List<SyntaxNodeBase> children)
    {
        switch (type)
        {
            case NonTerminatorType.ProgramStruct:
                return ProgramStruct.Create(children);
            case NonTerminatorType.ProgramHead:
                return ProgramHead.Create(children);
            case NonTerminatorType.ProgramBody:
                return ProgramBody.Create(children);
            case NonTerminatorType.IdentifierList:
                return IdentifierList.Create(children);
            case NonTerminatorType.VarDeclarations:
                return VarDeclarations.Create(children);
            case NonTerminatorType.SubprogramDeclarations:
                return SubprogramDeclarations.Create(children);
            case NonTerminatorType.CompoundStatement:
                return CompoundStatement.Create(children);
            case NonTerminatorType.ConstValue:
                return ConstValue.Create(children);
            case NonTerminatorType.ConstDeclaration:
                return ConstDeclaration.Create(children);
            case NonTerminatorType.ConstDeclarations:
                return ConstDeclarations.Create(children);
            case NonTerminatorType.VarDeclaration:
                return VarDeclaration.Create(children);
            case NonTerminatorType.Type:
                return TypeSyntaxNode.Create(children);
            case NonTerminatorType.BasicType:
                return BasicType.Create(children);
            case NonTerminatorType.Period:
                return Period.Create(children);
            case NonTerminatorType.Subprogram:
                return Subprogram.Create(children);
            case NonTerminatorType.SubprogramHead:
                return SubprogramHead.Create(children);
            case NonTerminatorType.SubprogramBody:
                return SubprogramBody.Create(children);
            case NonTerminatorType.FormalParameter:
                return FormalParameter.Create(children);
            case NonTerminatorType.ParameterList:
                return ParameterList.Create(children);
            case NonTerminatorType.Parameter:
                return Parameter.Create(children);
            case NonTerminatorType.VarParameter:
                return VarParameter.Create(children);
            case NonTerminatorType.ValueParameter:
                return ValueParameter.Create(children);
            case NonTerminatorType.StatementList:
                return StatementList.Create(children);
            case NonTerminatorType.Statement:
                return Statement.Create(children);
            case NonTerminatorType.Variable:
                return Variable.Create(children);
            case NonTerminatorType.Expression:
                return Expression.Create(children);
            case NonTerminatorType.ProcedureCall:
                return ProcedureCall.Create(children);
            case NonTerminatorType.ElsePart:
                return ElsePart.Create(children);
            case NonTerminatorType.ExpressionList:
                return ExpressionList.Create(children);
            case NonTerminatorType.SimpleExpression:
                return SimpleExpression.Create(children);
            case NonTerminatorType.Term:
                return Term.Create(children);
            case NonTerminatorType.Factor:
                return Factor.Create(children);
            case NonTerminatorType.AddOperator:
                return AddOperator.Create(children);
            case NonTerminatorType.MultiplyOperator:
                return MultiplyOperator.Create(children);
            case NonTerminatorType.RelationOperator:
                return RelationOperator.Create(children);
            case NonTerminatorType.IdVarPart:
                return IdentifierVarPart.Create(children);
            default:
                throw new InvalidOperationException();
        }
    }
}
