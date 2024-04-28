using Canon.Core.Abstractions;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;
using Canon.Core.SemanticParser;

namespace Canon.Core.SyntaxNodes;

public class Variable : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.Variable;

    private PascalType? _variableType;

    /// <summary>
    /// Variable实际的类型,用于数组赋值
    /// </summary>
    public PascalType VariableType
    {
        get
        {
            if (_variableType is null)
            {
                throw new InvalidOperationException();
            }

            return _variableType;
        }
        set
        {
            _variableType = value;
        }
    }

    /// <summary>
    /// 变量的名称
    /// </summary>
    public IdentifierSemanticToken Identifier =>
        (IdentifierSemanticToken)Children[0].Convert<TerminatedSyntaxNode>().Token;

    /// <summary>
    /// 声明数组访问的部分
    /// </summary>
    public IdentifierVarPart VarPart => Children[1].Convert<IdentifierVarPart>();

    public override void PreVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PreVisit(this);
    }

    public override void PostVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PostVisit(this);
    }

    public static Variable Create(List<SyntaxNodeBase> children)
    {
        return new Variable { Children = children };
    }
}
