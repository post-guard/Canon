using System.Diagnostics.CodeAnalysis;
using System.Security;
using Canon.Core.Exceptions;

namespace Canon.Core.SemanticParser;

/// <summary>
/// 类型表
/// </summary>
public class TypeTable
{
    private readonly Dictionary<string, PascalType> _types = new()
    {
        { PascalBasicType.Integer.TypeName, PascalBasicType.Integer },
        { PascalBasicType.Boolean.TypeName, PascalBasicType.Boolean },
        { PascalBasicType.Character.TypeName, PascalBasicType.Character },
        { PascalBasicType.Real.TypeName, PascalBasicType.Real }
    };

    /// <summary>
    /// 根据类型名称查找类型表
    /// </summary>
    /// <param name="typeName">想要查找的类型名称</param>
    /// <param name="type">查找到的类型</param>
    /// <returns>是否查找到类型</returns>
    public bool TryGetType(string typeName, [NotNullWhen(true)] out PascalType? type)
    {
        if (!_types.ContainsKey(typeName))
        {
            type = null;
            return false;
        }

        type = _types[typeName];
        return true;
    }
}
