using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class Term : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.Term;

    public static Term Create(List<SyntaxNodeBase> children)
    {
        return new Term { Children = children };
    }
}
