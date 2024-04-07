using Canon.Core.SemanticParser;

namespace Canon.Tests.SemanticTests;

public class SymbolTableTests
{
    [Fact]
    public void BasicTypeTest()
    {
        SymbolTable table = new();

        Assert.True(table.TryGetType("integer", out PascalType? integer));
        Assert.Equal(PascalBasicType.Integer, integer);
        Assert.True(table.TryGetType("real", out PascalType? real));
        Assert.Equal(PascalBasicType.Real, real);
        Assert.True(table.TryGetType("boolean", out PascalType? boolean));
        Assert.Equal(PascalBasicType.Boolean, boolean);
        Assert.True(table.TryGetType("char", out PascalType? character));
        Assert.Equal(PascalBasicType.Character, character);
    }

    [Fact]
    public void SingleTableInsertAndFindTest()
    {
        SymbolTable table = new();

        Assert.True(table.TryAddSymbol(new Symbol { SymbolName = "a", SymbolType = PascalBasicType.Integer }));
        Assert.True(table.TryAddSymbol(new Symbol
        {
            SymbolName = "temperature", SymbolType = PascalBasicType.Real, Const = true
        }));

        Assert.True(table.TryGetSymbol("a", out Symbol? a));
        Assert.Equal(PascalBasicType.Integer, a.SymbolType);

        Assert.False(table.TryGetSymbol("notExist", out a));
    }

    [Fact]
    public void NestedTableInsertAndFindTest()
    {
        SymbolTable table = new();

        Assert.True(table.TryAddSymbol(new Symbol { SymbolName = "a", SymbolType = PascalBasicType.Integer }));
        Assert.True(table.TryAddSymbol(new Symbol
        {
            SymbolName = "temperature", SymbolType = PascalBasicType.Real, Const = true
        }));

        SymbolTable child = table.CreateChildTable();

        Assert.True(child.TryAddSymbol(new Symbol{SymbolName = "a", SymbolType = PascalBasicType.Real}));
        Assert.True(child.TryAddSymbol(new Symbol
        {
            SymbolName = "level2", SymbolType = PascalBasicType.Boolean, Reference = true
        }));

        Assert.True(child.TryGetSymbol("a", out Symbol? a));
        Assert.Equal(PascalBasicType.Real, a.SymbolType);
        Assert.True(table.TryGetSymbol("a", out a));
        Assert.Equal(PascalBasicType.Integer, a.SymbolType);

        Assert.True(table.TryGetSymbol("temperature", out Symbol? temp));
        Assert.Equal(PascalBasicType.Real, temp.SymbolType);
    }
}
