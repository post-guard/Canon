using Canon.Core.CodeGenerators;
using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class SimpleExpression : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.SimpleExpression;

    public static SimpleExpression Create(List<SyntaxNodeBase> children)
    {
        return new SimpleExpression { Children = children };
    }

    public override void GenerateCCode(CCodeBuilder builder)
    {
        foreach (var child in Children)
        {
            child.GenerateCCode(builder);
        }
    }
}
