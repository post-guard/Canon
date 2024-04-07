using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class FormalParameter : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.FormalParameter;

    /// <summary>
    /// 声明的参数列表
    /// </summary>
    public IEnumerable<Parameter> Parameters => GetParameters();

    public static FormalParameter Create(List<SyntaxNodeBase> children)
    {
        return new FormalParameter { Children = children };
    }

    private IEnumerable<Parameter> GetParameters()
    {
        if (Children.Count == 0)
        {
            yield break;
        }

        foreach (Parameter parameter in Children[1].Convert<ParameterList>().Parameters)
        {
            yield return parameter;
        }
    }
}
