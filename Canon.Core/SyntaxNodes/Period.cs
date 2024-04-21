using Canon.Core.CodeGenerators;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Core.SyntaxNodes;

public class Period : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.Period;

    public bool IsRecursive { get; private init; }

    /// <summary>
    /// 数组上下界列表
    /// </summary>
    public IEnumerable<(NumberSemanticToken, NumberSemanticToken)> Ranges => GetRanges();

    public static Period Create(List<SyntaxNodeBase> children)
    {
        bool isRecursive;

        if (children.Count == 3)
        {
            isRecursive = false;
        }
        else if (children.Count == 5)
        {
            isRecursive = true;
        }
        else
        {
            throw new InvalidOperationException();
        }

        return new Period { Children = children, IsRecursive = isRecursive };
    }

    private (NumberSemanticToken, NumberSemanticToken) GetRange()
    {
        if (IsRecursive)
        {
            return ((NumberSemanticToken)Children[2].Convert<TerminatedSyntaxNode>().Token,
                (NumberSemanticToken)Children[4].Convert<TerminatedSyntaxNode>().Token);
        }
        else
        {
            return ((NumberSemanticToken)Children[0].Convert<TerminatedSyntaxNode>().Token,
                (NumberSemanticToken)Children[2].Convert<TerminatedSyntaxNode>().Token);
        }
    }

    private IEnumerable<(NumberSemanticToken, NumberSemanticToken)> GetRanges()
    {
        Period period = this;

        while (true)
        {
            if (period.IsRecursive)
            {
                yield return period.GetRange();
                period = period.Children[0].Convert<Period>();
            }
            else
            {
                yield return period.GetRange();
                break;
            }
        }
    }
}
