using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class IdentifierVarPart : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.IdVarPart;

    /// <summary>
    /// 是否声明了索引部分
    /// </summary>
    public bool Exist { get; private init; }

    /// <summary>
    /// 索引中的位置声明
    /// </summary>
    public IEnumerable<Expression> Positions => GetPositions();

    private IEnumerable<Expression> GetPositions()
    {
        if (!Exist)
        {
            yield break;
        }

        foreach (Expression expression in Children[1].Convert<ExpressionList>().Expressions)
        {
            yield return expression;
        }
    }

    public static IdentifierVarPart Create(List<SyntaxNodeBase> children)
    {
        bool exist;

        if (children.Count == 0)
        {
            exist = false;
        }
        else if (children.Count == 3)
        {
            exist = true;
        }
        else
        {
            throw new InvalidOperationException();
        }

        return new IdentifierVarPart { Children = children, Exist = exist };
    }
}
