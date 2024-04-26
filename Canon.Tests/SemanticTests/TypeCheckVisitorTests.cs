using Canon.Core.SemanticParser;
using Canon.Core.SyntaxNodes;
using Canon.Tests.Utils;

namespace Canon.Tests.SemanticTests;

public class TypeCheckVisitorTests
{
    [Fact]
    public void ConstTypeTest()
    {
        const string program = """
                               program main;
                               const a = 1; b = 1.23; c = 'a';
                               begin
                               end.
                               """;

        TypeCheckVisitor visitor = CheckType(program);

        Assert.True(visitor.SymbolTable.TryGetSymbol("a", out Symbol? symbol));
        Assert.Equal(PascalBasicType.Integer, symbol.SymbolType);

        Assert.True(visitor.SymbolTable.TryGetSymbol("b", out symbol));
        Assert.Equal(PascalBasicType.Real, symbol.SymbolType);

        Assert.True(visitor.SymbolTable.TryGetSymbol("c", out symbol));
        Assert.Equal(PascalBasicType.Character, symbol.SymbolType);
    }

    [Fact]
    public void SingleTypeTest()
    {
        const string program = """
                               program main;
                               var a : integer; b : char; c : boolean; d : real;
                               begin
                               end.
                               """;

        TypeCheckVisitor visitor = CheckType(program);

        Assert.True(visitor.SymbolTable.TryGetSymbol("a", out Symbol? symbol));
        Assert.Equal(PascalBasicType.Integer, symbol.SymbolType);

        Assert.True(visitor.SymbolTable.TryGetSymbol("b", out symbol));
        Assert.Equal(PascalBasicType.Character, symbol.SymbolType);

        Assert.True(visitor.SymbolTable.TryGetSymbol("c", out symbol));
        Assert.Equal(PascalBasicType.Boolean, symbol.SymbolType);

        Assert.True(visitor.SymbolTable.TryGetSymbol("d", out symbol));
        Assert.Equal(PascalBasicType.Real, symbol.SymbolType);
    }

    [Fact]
    public void MulitpleTypeTest()
    {
        const string program = """
                               program main;
                               var a, b, c, d : integer;
                               e, f, g : boolean;
                               begin
                               end.
                               """;

        TypeCheckVisitor visitor = CheckType(program);

        IEnumerable<string> names = ["a", "b", "c", "d"];
        foreach (string name in names)
        {
            Assert.True(visitor.SymbolTable.TryGetSymbol(name, out Symbol? symbol));
            Assert.Equal(PascalBasicType.Integer, symbol.SymbolType);
        }

        names = ["e", "f", "g"];
        foreach (string name in names)
        {
            Assert.True(visitor.SymbolTable.TryGetSymbol(name, out Symbol? symbol));
            Assert.Equal(PascalBasicType.Boolean, symbol.SymbolType);
        }
    }

    [Fact]
    public void ArrayTest()
    {
        const string program = """
                               program main;
                               var a : array [0..10] of integer;
                               b : array [0..10, 0..20] of integer;
                               c : array [100..200, 1..5] of real;
                               begin
                               end.
                               """;

        TypeCheckVisitor visitor = CheckType(program);

        Assert.True(visitor.SymbolTable.TryGetSymbol("a", out Symbol? symbol));
        Assert.Equal(symbol.SymbolType, new PascalArrayType(PascalBasicType.Integer, 0, 10));

        Assert.True(visitor.SymbolTable.TryGetSymbol("b", out symbol));
        Assert.Equal(symbol.SymbolType,
            new PascalArrayType(new PascalArrayType(PascalBasicType.Integer, 0, 20), 0, 10));

        Assert.True(visitor.SymbolTable.TryGetSymbol("c", out symbol));
        Assert.Equal(symbol.SymbolType, new PascalArrayType(
            new PascalArrayType(PascalBasicType.Real, 1, 5), 100, 200));
    }

    [Fact]
    public void ProcedureParameterTest()
    {
        const string program = """
                               program main;
                               procedure test(a, b, c : integer);
                               begin
                               end;
                               begin
                               end.
                               """;

        TypeCheckVisitor visitor = CheckType(program);

        Assert.True(visitor.SymbolTable.TryGetSymbol("test", out Symbol? symbol));
        Assert.Equal(symbol.SymbolType, new PascalFunctionType([
            new PascalParameterType(PascalBasicType.Integer, false),
            new PascalParameterType(PascalBasicType.Integer, false),
            new PascalParameterType(PascalBasicType.Integer, false)
        ], PascalBasicType.Void));
    }

    [Fact]
    public void ProcedureVarParameterTest()
    {
        const string program = """
                               program main;
                               procedure test(var a, b, c : real);
                               begin
                               end;
                               begin
                               end.
                               """;

        TypeCheckVisitor visitor = CheckType(program);

        Assert.True(visitor.SymbolTable.TryGetSymbol("test", out Symbol? symbol));
        Assert.Equal(symbol.SymbolType, new PascalFunctionType([
            new PascalParameterType(PascalBasicType.Real, true),
            new PascalParameterType(PascalBasicType.Real, true),
            new PascalParameterType(PascalBasicType.Real, true)
        ], PascalBasicType.Void));
    }

    [Fact]
    public void ProcedureBothParameterTest()
    {
        const string program = """
                               program main;
                               procedure test(a, b : integer; var c, d: char);
                               begin
                               end;
                               begin
                               end.
                               """;

        TypeCheckVisitor visitor = CheckType(program);

        Assert.True(visitor.SymbolTable.TryGetSymbol("test", out Symbol? symbol));
        Assert.Equal(symbol.SymbolType, new PascalFunctionType([
            new PascalParameterType(PascalBasicType.Integer, false),
            new PascalParameterType(PascalBasicType.Integer, false),
            new PascalParameterType(PascalBasicType.Character, true),
            new PascalParameterType(PascalBasicType.Character, true)
        ], PascalBasicType.Void));
    }

    [Fact]
    public void FunctionBothParameterTest()
    {
        const string program = """
                               program main;
                               function test(a, b : integer; var c, d: char) : real;
                               begin
                               end;
                               begin
                               end.
                               """;

        TypeCheckVisitor visitor = CheckType(program);

        Assert.True(visitor.SymbolTable.TryGetSymbol("test", out Symbol? symbol));
        Assert.Equal(symbol.SymbolType, new PascalFunctionType([
            new PascalParameterType(PascalBasicType.Integer, false),
            new PascalParameterType(PascalBasicType.Integer, false),
            new PascalParameterType(PascalBasicType.Character, true),
            new PascalParameterType(PascalBasicType.Character, true)
        ], PascalBasicType.Real));
    }

    [Fact]
    public void ProcedureAndFunctionTest()
    {
        const string program = """
                               program main;
                               procedure test1(a : integer; var b, c : real; d: boolean);
                               begin
                               end;
                               function test2(var a, b : boolean) : boolean;
                               begin
                               end;
                               begin
                               end.
                               """;

        TypeCheckVisitor visitor = CheckType(program);

        Assert.True(visitor.SymbolTable.TryGetSymbol("test1", out Symbol? symbol));
        Assert.Equal(symbol.SymbolType, new PascalFunctionType([
            new PascalParameterType(PascalBasicType.Integer, false),
            new PascalParameterType(PascalBasicType.Real, true),
            new PascalParameterType(PascalBasicType.Real, true),
            new PascalParameterType(PascalBasicType.Boolean, false)
        ], PascalBasicType.Void));

        Assert.True(visitor.SymbolTable.TryGetSymbol("test2", out symbol));
        Assert.Equal(symbol.SymbolType, new PascalFunctionType([
            new PascalParameterType(PascalBasicType.Boolean, true),
            new PascalParameterType(PascalBasicType.Boolean, true)
        ], PascalBasicType.Boolean));
    }

    /// <summary>
    /// 验证函数中的符号表是否正确
    /// </summary>
    private class SubprogramSymbolTableTestVisitor : TypeCheckVisitor
    {
        public override void PostVisit(SubprogramBody subprogramBody)
        {
            base.PostVisit(subprogramBody);

            Assert.True(SymbolTable.TryGetSymbol("a", out Symbol? symbol));
            Assert.Equal(PascalBasicType.Boolean, symbol.SymbolType);

            // 递归查父符号表
            Assert.True(SymbolTable.TryGetSymbol("b", out symbol));
            Assert.Equal(PascalBasicType.Real, symbol.SymbolType);

            Assert.True(SymbolTable.TryGetSymbol("c", out symbol));
            Assert.Equal(PascalBasicType.Character, symbol.SymbolType);

            Assert.True(SymbolTable.TryGetSymbol("d", out symbol));
            Assert.Equal(PascalBasicType.Character, symbol.SymbolType);
        }
    }

    [Fact]
    public void SubprogramSymbolTableTest()
    {
        const string program = """
                               program main;
                               const a = 3;
                               var b, c : real;
                               procedure test(a : boolean);
                               var c, d : char;
                               begin
                               end;
                               begin
                               end.
                               """;

        ProgramStruct root = CompilerHelpers.Analyse(program);
        SubprogramSymbolTableTestVisitor visitor = new();
        SyntaxTreeTraveller traveller = new();

        traveller.Travel(root, visitor);

        Assert.True(visitor.SymbolTable.TryGetSymbol("a", out Symbol? symbol));
        Assert.Equal(PascalBasicType.Integer, symbol.SymbolType);
        Assert.True(symbol.Const);

        Assert.True(visitor.SymbolTable.TryGetSymbol("b", out symbol));
        Assert.Equal(PascalBasicType.Real, symbol.SymbolType);

        Assert.True(visitor.SymbolTable.TryGetSymbol("c", out symbol));
        Assert.Equal(PascalBasicType.Real, symbol.SymbolType);

        Assert.True(visitor.SymbolTable.TryGetSymbol("test", out symbol));
        Assert.Equal(
            new PascalFunctionType([
                new PascalParameterType(PascalBasicType.Boolean, false)
            ], PascalBasicType.Void), symbol.SymbolType);

        Assert.False(visitor.SymbolTable.TryGetSymbol("d", out symbol));
    }

    private static TypeCheckVisitor CheckType(string program)
    {
        ProgramStruct root = CompilerHelpers.Analyse(program);
        TypeCheckVisitor visitor = new();
        SyntaxTreeTraveller traveller = new();

        traveller.Travel(root, visitor);

        return visitor;
    }
}
