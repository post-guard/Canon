using Canon.Core.CodeGenerators;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;
using Canon.Core.SemanticParser;

namespace Canon.Core.SyntaxNodes;

public class BasicType : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.BasicType;

    public static BasicType Create(List<SyntaxNodeBase> children)
    {
        return new BasicType { Children = children };
    }

    public override void GenerateCCode(CCodeBuilder builder)
    {
        var keywordType = Children[0].Convert<TerminatedSyntaxNode>().Token
            .Convert<KeywordSemanticToken>().KeywordType;

        switch (keywordType)
        {
            case KeywordType.Integer:
                builder.AddString(" int");
                break;
            case KeywordType.Real:
                builder.AddString(" double");
                break;
            case KeywordType.Boolean:
                builder.AddString(" bool");
                break;
            case KeywordType.Character:
                builder.AddString(" char");
                break;
        }
    }

    /// <summary>
    ///尝试获取Pascal的基本类型
    /// </summary>
    /// <returns></returns>
    public PascalType TryGetPascalType()
    {
        var keywordType = Children[0].Convert<TerminatedSyntaxNode>().Token
            .Convert<KeywordSemanticToken>().KeywordType;

        switch (keywordType)
        {
            case KeywordType.Integer:
                return PascalBasicType.Integer;
            case KeywordType.Real:
                return PascalBasicType.Real;
            case KeywordType.Boolean:
                return PascalBasicType.Boolean;
            case KeywordType.Character:
                return PascalBasicType.Character;
        }

        return PascalBasicType.Void;
    }
}
