using System.Diagnostics.CodeAnalysis;

namespace Canon.Core.SemanticParser;

/// <summary>
///符号表类
/// </summary>
public class SymbolTable
{
    /// <summary>
    /// 符号表
    /// </summary>
    private readonly Dictionary<string, Symbol> _symbols = [];

    /// <summary>
    /// 类型表
    /// </summary>
    private readonly TypeTable _typeTable = new();

    /// <summary>
    /// 父符号表
    /// </summary>
    private readonly SymbolTable? _parent;

    /// <summary>
    /// 获得当前符号表的所有父符号表
    /// </summary>
    public IEnumerable<SymbolTable> ParentTables => GetParents();

    public SymbolTable() {}

    private SymbolTable(SymbolTable parent)
    {
        _parent = parent;
    }

    public SymbolTable CreateChildTable()
    {
        return new SymbolTable(this);
    }

    /// <summary>
    /// 尝试向符号表中添加符号
    /// </summary>
    /// <param name="symbol">欲添加的符号</param>
    /// <returns>是否添加成功</returns>
    public bool TryAddSymbol(Symbol symbol)
    {
        if (_symbols.ContainsKey(symbol.SymbolName))
        {
            return false;
        }

        _symbols.Add(symbol.SymbolName, symbol);
        return true;
    }

    /// <summary>
    /// 尝试从符号表极其父符号表查找符号
    /// </summary>
    /// <param name="name">需要查找的符号名称</param>
    /// <param name="symbol">查找到的符号</param>
    /// <returns>是否查找到符号</returns>
    public bool TryGetSymbol(string name, [NotNullWhen(true)] out Symbol? symbol)
    {
        if (_symbols.TryGetValue(name, out symbol))
        {
            return true;
        }

        foreach (SymbolTable table in ParentTables)
        {
            if (table._symbols.TryGetValue(name, out symbol))
            {
                return true;
            }
        }

        symbol = null;
        return false;
    }

    /// <summary>
    /// 从符号表极其父表的类型表中查找类型
    /// </summary>
    /// <param name="typeName">欲查找的类型名称</param>
    /// <param name="type">查找到的类型</param>
    /// <returns>是否查找到类型</returns>
    public bool TryGetType(string typeName, [NotNullWhen(true)] out PascalType? type)
    {
        if (_typeTable.TryGetType(typeName, out type))
        {
            return true;
        }

        foreach (SymbolTable parent in ParentTables)
        {
            if (parent._typeTable.TryGetType(typeName, out type))
            {
                return true;
            }
        }

        type = null;
        return false;
    }

    /// <summary>
    /// 尝试获得父符号表
    /// </summary>
    /// <param name="parent">获得的父符号表</param>
    /// <returns>是否存在父符号表</returns>
    public bool TryGetParent([NotNullWhen(true)] out SymbolTable? parent)
    {
        if (_parent is null)
        {
            parent = null;
            return false;
        }

        parent = _parent;
        return true;
    }

    private IEnumerable<SymbolTable> GetParents()
    {
        SymbolTable? now = _parent;

        while (now is not null)
        {
            yield return now;
            now = now._parent;
        }
    }
}
