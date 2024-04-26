using Canon.Core.Abstractions;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Core.SyntaxNodes;

public class Period : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.Period;

    /// <summary>
    /// 所有定义的Period
    /// </summary>
    public List<Period> Periods { get; } = [];

    /// <summary>
    /// 数组的开始下标和结束下标
    /// </summary>
    public (NumberSemanticToken, NumberSemanticToken) Range
    {
        get
        {
            if (Children.Count == 3)
            {
                return (Children[0].Convert<TerminatedSyntaxNode>().Token.Convert<NumberSemanticToken>(),
                    Children[2].Convert<TerminatedSyntaxNode>().Token.Convert<NumberSemanticToken>());
            }
            else
            {
                return (Children[2].Convert<TerminatedSyntaxNode>().Token.Convert<NumberSemanticToken>(),
                    Children[4].Convert<TerminatedSyntaxNode>().Token.Convert<NumberSemanticToken>());
            }
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

    public static Period Create(List<SyntaxNodeBase> children)
    {
        Period result = new() { Children = children };

        if (children.Count == 3)
        {
            result.Periods.Add(result);
        }
        else
        {
            Period child = children[0].Convert<Period>();

            foreach (Period period in child.Periods)
            {
                result.Periods.Add(period);
            }

            result.Periods.Add(result);
        }

        return result;
    }
}
