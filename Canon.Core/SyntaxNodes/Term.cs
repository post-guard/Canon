using Canon.Core.CodeGenerators;
using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class Term : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.Term;

    public static Term Create(List<SyntaxNodeBase> children)
    {
        return new Term { Children = children };
    }

    public override void GenerateCCode(CCodeBuilder builder)
    {
        foreach (var child in Children)
        {
            child.GenerateCCode(builder);
        }
    }
}
