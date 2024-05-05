using Canon.Core.Abstractions;
using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class ProgramBody : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.ProgramBody;

    /// <summary>
    /// 语句声明
    /// </summary>
    public CompoundStatement CompoundStatement => Children[3].Convert<CompoundStatement>();

    public override void PreVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PreVisit(this);
    }

    public override void PostVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PostVisit(this);
    }

    public static ProgramBody Create(List<SyntaxNodeBase> children)
    {
        return new ProgramBody { Children = children };
    }
}
