using Canon.Core.Abstractions;
using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class Parameter : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.Parameter;

    /// <summary>
    /// 是否为引用变量
    /// </summary>
    public bool IsVar { get; private init; }

    public override void PreVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PreVisit(this);
    }

    public override void PostVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PostVisit(this);
    }

    public static Parameter Create(List<SyntaxNodeBase> children)
    {
        NonTerminatedSyntaxNode node = children[0].Convert<NonTerminatedSyntaxNode>();

        bool isVar;
        if (node.Type == NonTerminatorType.VarParameter)
        {
            isVar = true;
        }
        else if (node.Type == NonTerminatorType.ValueParameter)
        {
            isVar = false;
        }
        else
        {
            throw new InvalidOperationException();
        }

        return new Parameter { Children = children, IsVar = isVar };
    }
}
