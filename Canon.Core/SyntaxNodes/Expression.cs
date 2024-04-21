using Canon.Core.CodeGenerators;
using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class Expression : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.Expression;

    public static Expression Create(List<SyntaxNodeBase> children)
    {
        return new Expression { Children = children };
    }

    public override void GenerateCCode(CCodeBuilder builder)
    {
        foreach (var child in Children)
        {
            child.GenerateCCode(builder);
        }
    }
}
