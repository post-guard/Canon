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

    public override string TypeName { get; }

    private PascalBasicType(BasicType basicType, string typeName)
    {
        Type = basicType;
        TypeName = typeName;
    }

    public override bool Equals(PascalType? other)
    {
        if (other is not PascalBasicType pascalBasicType)
        {
            return false;
        }

        return Type == pascalBasicType.Type;
    }

    public override int GetHashCode()
    {
        return Type.GetHashCode();
    }

    public override string ToString() => TypeName;

    /// <summary>
    /// 整数类型的单例对象
    /// </summary>
    public static PascalType Integer => new PascalBasicType(BasicType.Integer, "integer");

    /// <summary>
    /// 布尔类型的单例对象
    /// </summary>
    public static PascalType Boolean => new PascalBasicType(BasicType.Boolean, "boolean");

    /// <summary>
    /// 字符类型的单例对象
    /// </summary>
    public static PascalType Character => new PascalBasicType(BasicType.Character, "char");

    /// <summary>
    /// 浮点数类型的单例对象
    /// </summary>
    public static PascalType Real => new PascalBasicType(BasicType.Real, "real");

    /// <summary>
    /// 空类型的单例对象
    /// </summary>
    public static PascalType Void => new PascalBasicType(BasicType.Void, "void");
}
