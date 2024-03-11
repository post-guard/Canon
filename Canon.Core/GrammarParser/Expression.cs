namespace Canon.Core.GrammarParser;

/// <summary>
/// LR语法中的一个表达式，例如 'program_struct -> ~program_head ; program_body'
/// 其中'~'标识当前移进到达的位置
/// </summary>
public class Expression : IEquatable<Expression>
{
    /// <summary>
    /// 表达式的左部
    /// </summary>
    public required NonTerminator Left { get; init; }

    /// <summary>
    /// 表达式的向前看字符串
    /// </summary>
    public required Terminator LookAhead { get; init; }

    /// <summary>
    /// 表达式的右部
    /// </summary>
    public required List<TerminatorBase> Right { get; init; }

    /// <summary>
    /// 当前移进的位置
    /// </summary>
    public required int Pos { get; init; }

    public bool Equals(Expression? other)
    {
        if (other is null)
        {
            return false;
        }

        if (Right.Count != other.Right.Count)
        {
            return false;
        }

        for (int i = 0; i < Right.Count; i++)
        {
            if (Right[i].IsTerminated != other.Right[i].IsTerminated)
            {
                return false;
            }

            if (!Right[i].Equals(other.Right[i]))
            {
                return false;
            }
        }

        return Left == other.Left
               && LookAhead == other.LookAhead
               && Pos == other.Pos;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Expression other)
        {
            return false;
        }

        return Equals(other);
    }

    public override int GetHashCode()
    {
        int hash = Left.GetHashCode();
        hash ^= LookAhead.GetHashCode();
        hash ^= Pos.GetHashCode();

        foreach (TerminatorBase terminator in Right)
        {
            hash ^= terminator.GetHashCode();
        }

        return hash;
    }

    public override string ToString()
    {
        string result = $"{Left} -> ";

        for (int i = 0; i < Right.Count; i++)
        {
            if (i == Pos)
            {
                result += '~';
            }

            result += ' ';
            result += Right[i].ToString();
        }

        if (Pos == Right.Count)
        {
            result += '~';
        }

        result += $", {LookAhead}";

        return result;
    }

    public static bool operator ==(Expression a, Expression b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(Expression a, Expression b)
    {
        return !a.Equals(b);
    }
}
