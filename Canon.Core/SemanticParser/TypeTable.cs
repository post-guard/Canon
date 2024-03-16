using System.Security;
using Canon.Core.Enums;
using Canon.Core.Exceptions;

namespace Canon.Core.SemanticParser;
/// <summary>
/// 类型表
/// </summary>
public class TypeTable
{
    private Dictionary<string, IdentifierType> EntryDict { get; init; }

    public TypeTable()
    {
        EntryDict = new Dictionary<string, IdentifierType>();
        //加入4种基本类型
        EntryDict.Add("integer", new BasicType(BasicIdType.Int));
        EntryDict.Add("real", new BasicType(BasicIdType.Real));
        EntryDict.Add("char", new BasicType(BasicIdType.Char));
        EntryDict.Add("boolean", new BasicType(BasicIdType.Bool));
    }


    /// <summary>
    /// 判断类型表里是否已经有该类型
    /// </summary>
    /// <param name="typeName">类型名称</param>
    /// <returns>如果有，返回true</returns>
    public bool Check(string typeName)
    {
        return EntryDict.ContainsKey(typeName);
    }

    /// <summary>
    /// 往类型表里添加类型
    /// </summary>
    /// <param name="typeName">类型名称</param>
    /// <param name="identifierType">类型的类别(一般是记录)</param>
    public void AddEntry(string typeName, IdentifierType identifierType)
    {
        if (!Check(typeName))
        {
            EntryDict.Add(typeName, identifierType);
        }
        else
        {
            throw new SemanticException("Failed to add to TypeTable! Types were repeatedly defined");
        }
    }

    /// <summary>
    /// 由类型名获取类型
    /// </summary>
    public IdentifierType GetTypeByName(string typeName)
    {
        if (Check(typeName))
        {
            return EntryDict[typeName];
        }
        else
        {
            throw new SecurityException("Failed to get type from typeTable! Type is not existed");
        }
    }

}
