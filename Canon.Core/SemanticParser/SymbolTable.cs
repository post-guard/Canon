using Canon.Core.Exceptions;

namespace Canon.Core.SemanticParser;
/// <summary>
///符号表类
/// </summary>
public class SymbolTable
{
    public Dictionary<string, SymbolTableEntry> Entries;

    public TypeTable TypesTable;    //当前符号表对应的类型表

    public SymbolTable? PreTable;  //直接外围符号表

    public SymbolTable()
    {
        Entries = new Dictionary<string, SymbolTableEntry>();
        TypesTable = new TypeTable();
        PreTable = null;
    }

    public SymbolTable(SymbolTable preTable)
    {
        Entries = new Dictionary<string, SymbolTableEntry>();
        TypesTable = new TypeTable();
        PreTable = preTable;
    }

    /// <summary>
    /// 向符号表里插入一个表项
    /// </summary>
    public void AddEntry(string idName, IdentifierType type, bool isConst, bool isVarParam)
    {
        if (Check(idName))
        {
            throw new SemanticException("failed to insert to SymbolTable! " +  idName + " is defined repeatedly");
        }

        Entries.Add(idName, new SymbolTableEntry(idName, type, isConst, isVarParam));
    }

    public void AddEntry(string idName, IdentifierType type, SymbolTable subTable)
    {
        if (Check(idName))
        {
            throw new SemanticException("failed to insert to SymbolTable! " +  idName + " is defined repeatedly");
        }

        Entries.Add(idName, new SymbolTableEntry(idName, type, subTable));
    }


    /// <summary>
    ///检查符号表，看是否有变量重复声明
    /// </summary>
    /// <param name="idName">查询的id名称</param>
    /// <returns>如果变量重复声明，返回true</returns>
    public bool Check(string idName)
    {
        return Entries.ContainsKey(idName);
    }

    /// <summary>
    /// 在符号表里查找，看当前引用变量是否声明
    /// </summary>
    /// <param name="idName">查询的id名称</param>
    /// <returns>如果有定义，返回true</returns>
    public bool Find(string idName)
    {
        if (Entries.ContainsKey(idName))
        {
            return true;
        }
        if (PreTable is not null && PreTable.Entries.ContainsKey(idName))
        {
            return true;
        }

        throw new SemanticException("identifier "+ idName + " is not defined!");
    }

    /// <summary>
    /// 通过id名获取id的类型
    /// </summary>
    /// <param name="idName">id名字</param>
    /// <returns>id在符号表里的类型</returns>
    public IdentifierType GetIdTypeByName(string idName)
    {
        if (Entries.ContainsKey(idName))
        {
            return Entries[idName].Type;
        }
        if (PreTable is not null && PreTable.Entries.ContainsKey(idName))
        {
            return PreTable.Entries[idName].Type;
        }

        throw new SemanticException("identifier "+ idName + " is not defined!");
    }



    public bool IsConst(string idName)
    {
        return Find(idName) && Entries[idName].IsConst;
    }
}
