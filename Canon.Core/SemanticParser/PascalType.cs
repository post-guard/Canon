using Canon.Core.Enums;

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

    /// <summary>
    /// 将当前类型转换为引用类型
    /// 原有类型变量保持不变
    /// </summary>
    /// <returns>原有Pascal类型的引用类型</returns>
    public abstract PascalType ToReferenceType();

    /// <summary>
    /// 是否为引用类型
    /// </summary>
    public bool IsReference { get; init; }

    public virtual bool Equals(PascalType? other)
    {
        if (other is null)
        {
            return false;
        }

        return IsReference == other.IsReference;
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
        return IsReference.GetHashCode();
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
        if (a is not PascalBasicType aType || b is not PascalBasicType bType)
        {
            return PascalBasicType.Void;
        }

        if (aType.Type == BasicType.Boolean && bType.Type == BasicType.Boolean)
        {
            return PascalBasicType.Boolean;
        }

        if (aType.Type == BasicType.Integer && bType.Type == BasicType.Integer)
        {
            return PascalBasicType.Integer;
        }

        if ((aType.Type == BasicType.Real && bType.Type == BasicType.Real)
            || (aType.Type == BasicType.Real && bType.Type == BasicType.Integer)
            || (aType.Type == BasicType.Integer && bType.Type == BasicType.Real))
        {
            return PascalBasicType.Real;
        }

        return PascalBasicType.Void;
    }
}
