using Canon.Core.Enums;

namespace Canon.Core.SemanticParser;

/// <summary>
/// 标识符类型基类
/// </summary>
public abstract class IdentifierType;

public class BasicType : IdentifierType
{
    public BasicIdType IdType;

    public BasicType(BasicIdType basicIdType)
    {
        IdType = basicIdType;
    }

    public static bool operator ==(BasicType a, BasicType b)
    {
        return a.IdType == b.IdType;
    }

    public static bool operator !=(BasicType a, BasicType b)
    {
        return !(a == b);
    }
}

public class ArrayType : IdentifierType
{
    public int Dimension;

    public List<Limits> LimitsList;

    public IdentifierType ElementType;

    public ArrayType(int dimension, List<Limits> limitsList, IdentifierType elementType)
    {
        Dimension = dimension;
        LimitsList = limitsList;
        ElementType = elementType;
    }

    public static bool operator ==(ArrayType a, ArrayType b)
    {
        if (a.Dimension != b.Dimension || a.ElementType != b.ElementType || a.LimitsList.Count != b.LimitsList.Count)
        {
            return false;
        }

        int n = a.LimitsList.Count;
        for (int i = 0; i < n; i++)
        {
            if (a.LimitsList[i] != b.LimitsList[i])
            {
                return false;
            }
        }

        return true;
    }

    public static bool operator !=(ArrayType a, ArrayType b)
    {
        return !(a == b);
    }
}

public class FuncType : IdentifierType
{
    public List<Param> ParamTypeList;

    public IdentifierType ReturnType;

    public FuncType(List<Param> paramTypeList, IdentifierType returnType)
    {
        ParamTypeList = paramTypeList;
        ReturnType = returnType;
    }

}

public class ProcType : IdentifierType
{
    public List<Param> ParamTypeList;

    public ProcType()
    {
        ParamTypeList = new List<Param>();
    }

    public ProcType(List<Param> paramTypeList)
    {
        ParamTypeList = paramTypeList;
    }
}

public class RecordType : IdentifierType
{
    public Dictionary<string, IdentifierType> MemberDic;

    public RecordType()
    {
        MemberDic = new Dictionary<string, IdentifierType>();
    }

    public static bool operator ==(RecordType a, RecordType b)
    {
        if (a.MemberDic.Count != b.MemberDic.Count)
        {
            return false;
        }

        foreach (var k in a.MemberDic.Keys)
        {
            if (!b.MemberDic.ContainsKey(k) || a.MemberDic[k] != b.MemberDic[k])
            {
                return false;
            }
        }

        return true;
    }

    public static bool operator !=(RecordType a, RecordType b)
    {
        return !(a == b);
    }

}

/// <summary>
/// 空类型，用于充当procedure的返回值
/// </summary>
public class NonType : IdentifierType
{

}

public class Limits
{
    public uint LowerBound;

    public uint UpperBound;

    public Limits(uint lowerBound, uint upperBound)
    {
        LowerBound = lowerBound;
        UpperBound = upperBound;
    }

    public static bool operator ==(Limits a, Limits b)
    {
        return a.LowerBound == b.LowerBound && a.UpperBound == b.UpperBound;
    }

    public static bool operator !=(Limits a, Limits b)
    {
        return !(a == b);
    }
}

public class Param
{
    public string Name;

    public IdentifierType Type;

    public bool IsVar;

    public Param(string name, IdentifierType type, bool isVar)
    {
        Name = name;
        Type = type;
        IsVar = isVar;
    }
}
