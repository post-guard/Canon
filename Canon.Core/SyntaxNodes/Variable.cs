using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Core.SyntaxNodes;

public class Variable : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.Variable;

    /// <summary>
    /// 变量的名称
    /// </summary>
    public IdentifierSemanticToken Identifier =>
        (IdentifierSemanticToken)Children[0].Convert<TerminatedSyntaxNode>().Token;

    public static Variable Create(List<SyntaxNodeBase> children)
    {
        return new Variable { Children = children };
    }
}
