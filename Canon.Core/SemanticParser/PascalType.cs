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

    public T Convert<T>() where T : PascalType
    {
        if (this is T result)
        {
            return result;
        }

        throw new InvalidOperationException("Can not convert target PascalType");
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

    public static PascalType operator +(PascalType a, PascalType b)
    {
        if (!IsCalculatable(a) || !IsCalculatable(b))
        {
            throw new InvalidOperationException();
        }

        if (a == PascalBasicType.Boolean && b == PascalBasicType.Boolean)
        {
            return PascalBasicType.Boolean;
        }

        if (a == PascalBasicType.Integer && b == PascalBasicType.Integer)
        {
            return PascalBasicType.Integer;
        }
        else
        {
            return PascalBasicType.Real;
        }
    }

    /// <summary>
    /// 是否为可计算的类型
    /// </summary>
    /// <param name="pascalType">需要判断的Pascal类型</param>
    /// <returns>是否为可计算的类型</returns>
    public static bool IsCalculatable(PascalType pascalType)
    {
        return pascalType == PascalBasicType.Integer || pascalType == PascalBasicType.Real
                                                     || pascalType == PascalBasicType.Boolean;
    }
}
