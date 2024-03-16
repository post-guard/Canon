namespace Canon.Core.SemanticParser;
/// <summary>
/// 符号表表项类
/// </summary>
public class SymbolTableEntry
{
    public string Name;

    public IdentifierType Type;

    public bool IsConst;    //是否为常量

    public bool IsVarParam;     //是否为引用变量

    public SymbolTable? SubTable;  //当前表项的子表

    public SymbolTableEntry(string name, IdentifierType type, bool isConst, bool isVarParam)
    {
        Name = name;
        Type = type;
        IsConst = isConst;
        IsVarParam = isVarParam;
    }

    public SymbolTableEntry(string name, IdentifierType type, SymbolTable subTable)
    {
        Name = name;
        Type = type;
        IsConst = false;
        IsVarParam = false;
        SubTable = subTable;
    }
}




