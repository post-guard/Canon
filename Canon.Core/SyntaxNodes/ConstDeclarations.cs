using Canon.Core.Abstractions;
using Canon.Core.CodeGenerators;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Core.SyntaxNodes;

public class ConstDeclarations : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.ConstDeclarations;

    /// <summary>
    /// 声明的常量列表
    /// </summary>
    public IEnumerable<(IdentifierSemanticToken, ConstValue)> ConstValues => GetConstValues();

    public override void PreVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PreVisit(this);
    }

    public override void PostVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PostVisit(this);
    }

    public static ConstDeclarations Create(List<SyntaxNodeBase> children)
    {
        return new ConstDeclarations { Children = children };
    }

    private IEnumerable<(IdentifierSemanticToken, ConstValue)> GetConstValues()
    {
        if (Children.Count == 0)
        {
            yield break;
        }

        ConstDeclaration declaration = Children[1].Convert<ConstDeclaration>();

        while (true)
        {
            yield return declaration.ConstValue;

            if (declaration.IsRecursive)
            {
                declaration = declaration.Children[0].Convert<ConstDeclaration>();
            }
            else
            {
                break;
            }
        }
    }

    public override void GenerateCCode(CCodeBuilder builder)
    {
        foreach (var pair in ConstValues.Reverse())
        {
            builder.AddString(" const");
            //获取常量类型
            var token = pair.Item2.Children[0].Convert<TerminatedSyntaxNode>().Token;
            var tokenType = token.TokenType;
            if (tokenType == SemanticTokenType.Number)
            {
                if (token.Convert<NumberSemanticToken>().NumberType == NumberType.Integer)
                {
                    builder.AddString(" int ");
                }
                else
                {
                    builder.AddString(" double ");
                }
            }
            else if (tokenType == SemanticTokenType.Character)
            {
                builder.AddString(" char ");
            }
            else
            {
                builder.AddString(" bool ");
            }

            builder.AddString(pair.Item1.IdentifierName + " =");
            pair.Item2.GenerateCCode(builder);
            builder.AddString(";");
        }
    }
}
