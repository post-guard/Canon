namespace Canon.Core.SemanticParser;

/// <summary>
/// Pascal类型基类
/// </summary>
public abstract class PascalType : IEquatable<PascalType>
{
    /// <summary>
    /// 类型的名称
    /// </summary>
    public abstract string TypeName { get; }

    public virtual bool Equals(PascalType? other)
    {
        if (other is null)
        {
            return false;
        }

        return TypeName == other.TypeName;
    }

    public override bool Equals(object? obj)
    {
        if (obj is PascalType other)
        {
            return Equals(other);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return TypeName.GetHashCode();
    }

    public static bool operator ==(PascalType a, PascalType b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(PascalType a, PascalType b)
    {
        return !a.Equals(b);
    }
}
