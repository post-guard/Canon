using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Core.SyntaxNodes;

public class ProgramHead : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.ProgramHead;

    /// <summary>
    /// 程序名称
    /// </summary>
    public IdentifierSemanticToken ProgramName
        => (IdentifierSemanticToken)Children[1].Convert<TerminatedSyntaxNode>().Token;

    /// <summary>
    /// 暂时意义不明的标识符列表
    /// https://wiki.freepascal.org/Program_Structure/zh_CN
    /// TODO: 查阅资料
    /// </summary>
    public IEnumerable<IdentifierSemanticToken> FileList => GetFileList();

    public static ProgramHead Create(List<SyntaxNodeBase> children)
    {
        return new ProgramHead { Children = children };
    }

    private IEnumerable<IdentifierSemanticToken> GetFileList()
    {
        if (Children.Count == 2)
        {
            yield break;
        }

        foreach (IdentifierSemanticToken token in Children[3].Convert<IdentifierList>().Identifiers)
        {
            yield return token;
        }
    }
}
