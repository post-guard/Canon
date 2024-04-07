namespace Canon.Core.SemanticParser;

/// <summary>
/// 符号表表项类
/// </summary>
public class Symbol : IEquatable<Symbol>
{
    /// <summary>
    /// 符号的名称
    /// </summary>
    public required string SymbolName { get; init; }

    /// <summary>
    /// 符号的类型
    /// </summary>
    public required PascalType SymbolType { get; init; }

    /// <summary>
    /// 是否为常量
    /// </summary>
    public bool Const { get; init; }

    /// <summary>
    /// 是否为引用变量
    /// </summary>
    public bool Reference { get; init; }

    public bool Equals(Symbol? other)
    {
        if (other is null)
        {
            return false;
        }

        return SymbolName == other.SymbolName
               && SymbolType == other.SymbolType
               && Const == other.Const
               && Reference == other.Reference;
    }

    public override int GetHashCode()
    {
        return SymbolName.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Symbol other)
        {
            return false;
        }

        return Equals(other);
    }
}



