namespace Canon.Core.GrammarParser;

/// <summary>
/// LR语法中的一个项目集规范族
/// 也就是自动机中的一个状态
/// </summary>
public class LrState : IEquatable<LrState>
{
    /// <summary>
    /// 项目集规范族
    /// </summary>
    public required HashSet<Expression> Expressions { get; init; }

    /// <summary>
    /// 自动机的迁移规则
    /// </summary>
    public Dictionary<TerminatorBase, LrState> Transformer { get; } = [];

    public bool Equals(LrState? other)
    {
        if (other is null)
        {
            return false;
        }

        if (Expressions.Count != other.Expressions.Count)
        {
            return false;
        }

        // 如果两个集合的大小相等，且一个是另一个的子集，那么两个集合相等。
        return Expressions.IsSubsetOf(other.Expressions);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not LrState other)
        {
            return false;
        }

        return Equals(other);
    }

    public override int GetHashCode()
    {
        int hash = 0;

        foreach (Expression expression in Expressions)
        {
            hash ^= expression.GetHashCode();
        }

        return hash;
    }

    public static bool operator ==(LrState a, LrState b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(LrState a, LrState b)
    {
        return !a.Equals(b);
    }
}
