using Canon.Core.Abstractions;
using Canon.Core.CodeGenerators;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;
using Canon.Core.SemanticParser;

namespace Canon.Core.SyntaxNodes;

public class BasicType : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.BasicType;

    /// <summary>
    /// BasicType代表的Pascal类型
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public PascalType PascalType
    {
        get
        {
            KeywordType keywordType = Children[0].Convert<TerminatedSyntaxNode>().Token
                .Convert<KeywordSemanticToken>().KeywordType;

            switch (keywordType)
            {
                case KeywordType.Integer:
                    return PascalBasicType.Integer;
                case KeywordType.Real:
                    return PascalBasicType.Real;
                case KeywordType.Character:
                    return PascalBasicType.Character;
                case KeywordType.Boolean:
                    return PascalBasicType.Boolean;
            }

            throw new InvalidOperationException();
        }
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

    public override void PreVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PreVisit(this);
    }

    public override void PostVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PostVisit(this);
    }

    public static BasicType Create(List<SyntaxNodeBase> children)
    {
        return new BasicType { Children = children };
    }
}
