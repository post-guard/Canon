using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class ParameterList : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.ParameterList;

    public bool IsRecursive { get; private init; }

    /// <summary>
    /// 声明的参数列表
    /// </summary>
    public IEnumerable<Parameter> Parameters => GetParameters();

    public static ParameterList Create(List<SyntaxNodeBase> children)
    {
        bool isRecursive;

        if (children.Count == 1)
        {
            isRecursive = false;
        }
        else if (children.Count == 3)
        {
            isRecursive = true;
        }
        else
        {
            throw new InvalidOperationException();
        }

        return new ParameterList { Children = children, IsRecursive = isRecursive };
    }

    private IEnumerable<Parameter> GetParameters()
    {
        ParameterList list = this;

        while (true)
        {
            if (list.IsRecursive)
            {
                yield return list.Children[2].Convert<Parameter>();
                list = list.Children[0].Convert<ParameterList>();
            }
            else
            {
                yield return list.Children[0].Convert<Parameter>();
                break;
            }
        }
    }
}
