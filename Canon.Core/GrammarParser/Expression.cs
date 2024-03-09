namespace Canon.Core.GrammarParser;

/// <summary>
/// An expression in the LR, like 'program_struct -> ~program_head ; program_body.'
/// The '~' is the shift position now.
/// </summary>
public class Expression : IEquatable<Expression>
{
    public required NonTerminator Left { get; init; }

    public required Terminator LookAhead { get; init; }

    public required List<TerminatorBase> Right { get; init; }

    public int Pos { get; set; }

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
               && LookAhead == other.LookAhead;
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

        foreach (TerminatorBase terminator in Right)
        {
            hash ^= terminator.GetHashCode();
        }

        return hash;
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
