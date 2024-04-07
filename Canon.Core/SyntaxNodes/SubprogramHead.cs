using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Core.SyntaxNodes;

public class SubprogramHead : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.SubprogramHead;

    public bool IsProcedure { get; private init; }

    /// <summary>
    /// 子程序的名称
    /// </summary>
    public IdentifierSemanticToken SubprogramName =>
        (IdentifierSemanticToken)Children[1].Convert<TerminatedSyntaxNode>().Token;

    /// <summary>
    /// 子程序的参数
    /// </summary>
    public IEnumerable<Parameter> Parameters => Children[2].Convert<FormalParameter>().Parameters;

    public static SubprogramHead Create(List<SyntaxNodeBase> children)
    {
        bool isProcedure;

        TerminatedSyntaxNode node = children[0].Convert<TerminatedSyntaxNode>();
        KeywordSemanticToken token = (KeywordSemanticToken)node.Token;

        if (token.KeywordType == KeywordType.Procedure)
        {
            isProcedure = true;
        }
        else if (token.KeywordType == KeywordType.Function)
        {
            isProcedure = false;
        }
        else
        {
            throw new InvalidOperationException();
        }

        return new SubprogramHead { Children = children, IsProcedure = isProcedure };
    }
}
