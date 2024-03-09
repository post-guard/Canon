using Canon.Core.Enums;

namespace Canon.Core.GrammarParser;

public abstract class TerminatorBase
{
    public abstract bool IsTerminated { get; }
}

/// <summary>
/// A terminator in grammar and it always represents a semantic token.
/// </summary>
public class Terminator : TerminatorBase, IEquatable<Terminator>
{
    public override bool IsTerminated => true;

    private readonly bool _isKeyword;

    private readonly KeywordType _keywordType;
    private readonly DelimiterType _delimiterType;

    public Terminator(KeywordType keywordType)
    {
        _isKeyword = true;
        _keywordType = keywordType;
    }

    public Terminator(DelimiterType delimiterType)
    {
        _isKeyword = false;
        _delimiterType = delimiterType;
    }

    public override int GetHashCode()
    {
        if (_isKeyword)
        {
            return _keywordType.GetHashCode();
        }
        else
        {
            return _delimiterType.GetHashCode();
        }
    }

    public bool Equals(Terminator? other)
    {
        if (other is null)
        {
            return false;
        }

        if (_isKeyword != other._isKeyword)
        {
            return false;
        }

        if (_isKeyword)
        {
            return _keywordType == other._keywordType;
        }
        else
        {
            return _delimiterType == other._delimiterType;
        }
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Terminator other)
        {
            return false;
        }

        return Equals(other);
    }

    public static bool operator ==(Terminator a, Terminator b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(Terminator a, Terminator b)
    {
        return !a.Equals(b);
    }
}

/// <summary>
/// A non-terminator in grammar like the 'ProgramStruct'.
/// </summary>
public class NonTerminator : TerminatorBase, IEquatable<NonTerminator>
{
    public override bool IsTerminated => false;

    private readonly NonTerminatorType _type;

    public NonTerminator(NonTerminatorType type)
    {
        _type = type;
    }

    public override int GetHashCode()
    {
        return _type.GetHashCode();
    }

    public bool Equals(NonTerminator? other)
    {
        if (other is null)
        {
            return false;
        }

        return _type == other._type;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not NonTerminator other)
        {
            return false;
        }

        return Equals(other);
    }

    public static bool operator ==(NonTerminator a, NonTerminator b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(NonTerminator a, NonTerminator b)
    {
        return !a.Equals(b);
    }
}
