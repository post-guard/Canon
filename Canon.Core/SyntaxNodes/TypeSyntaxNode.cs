using Canon.Core.CodeGenerators;
using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class TypeSyntaxNode : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.Type;

    public static TypeSyntaxNode Create(List<SyntaxNodeBase> children)
    {
        return new TypeSyntaxNode { Children = children };
    }

    public override void GenerateCCode(CCodeBuilder builder)
    {
        //type -> basic_type
        if (Children.Count == 1)
        {
            Children[0].GenerateCCode(builder);
        }
        //type -> array [ period ]of basic_type
        else
        {

        }
    }
}
