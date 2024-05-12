using Canon.Core.Enums;

namespace Canon.Core.SemanticParser;

/// <summary>
/// 基础Pascal类型
/// </summary>
public class PascalBasicType : PascalType
{
    /// <summary>
    /// 基础类型
    /// </summary>
    public BasicType Type { get; }

    public override string TypeName => Type.ToString();

    private PascalBasicType(BasicType type)
    {
        Type = type;
    }

    public override PascalType ToReferenceType()
    {
        return new PascalBasicType(Type) { IsReference = true };
    }

    public override bool Equals(PascalType? other)
    {
        if (other is not PascalBasicType pascalBasicType)
        {
            return false;
        }

        return Type == pascalBasicType.Type && base.Equals(pascalBasicType);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode() ^ Type.GetHashCode();
    }

    public override string ToString() => TypeName;

    /// <summary>
    /// 整数类型的单例对象
    /// </summary>
    public static PascalType Integer => new PascalBasicType(BasicType.Integer);

    /// <summary>
    /// 布尔类型的单例对象
    /// </summary>
    public static PascalType Boolean => new PascalBasicType(BasicType.Boolean);

    /// <summary>
    /// 字符类型的单例对象
    /// </summary>
    public static PascalType Character => new PascalBasicType(BasicType.Character);

    /// <summary>
    /// 浮点数类型的单例对象
    /// </summary>
    public static PascalType Real => new PascalBasicType(BasicType.Real);

    /// <summary>
    /// 空类型的单例对象
    /// </summary>
    public static PascalType Void => new PascalBasicType(BasicType.Void);
}
