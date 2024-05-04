using Canon.Core.SemanticParser;
using Canon.Core.SyntaxNodes;
using Canon.Tests.Utils;
using Xunit.Abstractions;

namespace Canon.Tests.SemanticTests;

public class TypeCheckVisitorTests(ITestOutputHelper testOutputHelper)
{
    private readonly TestLogger _logger = new(testOutputHelper);

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
            new PascalParameterType(PascalBasicType.Integer, false, "a"),
            new PascalParameterType(PascalBasicType.Integer, false, "a"),
            new PascalParameterType(PascalBasicType.Integer, false, "a")
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
            new PascalParameterType(PascalBasicType.Real, true, "a"),
            new PascalParameterType(PascalBasicType.Real, true, "a"),
            new PascalParameterType(PascalBasicType.Real, true, "a")
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
            new PascalParameterType(PascalBasicType.Integer, false, "a"),
            new PascalParameterType(PascalBasicType.Integer, false, "a"),
            new PascalParameterType(PascalBasicType.Character, true, "a"),
            new PascalParameterType(PascalBasicType.Character, true, "a")
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
            new PascalParameterType(PascalBasicType.Integer, false, "a"),
            new PascalParameterType(PascalBasicType.Integer, false, "a"),
            new PascalParameterType(PascalBasicType.Character, true, "a"),
            new PascalParameterType(PascalBasicType.Character, true, "a")
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
            new PascalParameterType(PascalBasicType.Integer, false, "a"),
            new PascalParameterType(PascalBasicType.Real, true, "a"),
            new PascalParameterType(PascalBasicType.Real, true, "a"),
            new PascalParameterType(PascalBasicType.Boolean, false, "a")
        ], PascalBasicType.Void));

        Assert.True(visitor.SymbolTable.TryGetSymbol("test2", out symbol));
        Assert.Equal(symbol.SymbolType, new PascalFunctionType([
            new PascalParameterType(PascalBasicType.Boolean, true, "a"),
            new PascalParameterType(PascalBasicType.Boolean, true, "a")
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
                new PascalParameterType(PascalBasicType.Boolean, false, "a")
            ], PascalBasicType.Void), symbol.SymbolType);

        Assert.False(visitor.SymbolTable.TryGetSymbol("d", out symbol));
    }

    [Fact]
    public void VarAssignStatementTest()
    {
        const string program = """
                               program main;
                               var
                               a : char;
                               b : integer;
                               begin
                               b := 3;
                               a := b;
                               end.
                               """;

        TypeCheckVisitor visitor = CheckType(program);
        Assert.True(visitor.IsError);
    }

    [Fact]
    public void TryConstAssignStatementTest()
    {
        const string program = """
                               program main;
                               const
                               a = 3;
                               var
                               b : integer;
                               begin
                               a := 4;
                               end.
                               """;

        TypeCheckVisitor visitor = CheckType(program);
        Assert.True(visitor.IsError);
    }

    [Fact]
    public void FunctionAssignStatementTest()
    {
        const string program = """
                               program exFunction;
                               var
                                  a, b, ret : integer;


                               function max(num1, num2: integer): integer;
                               var
                                    error:char;
                                  result: integer;

                               begin
                                  if (num1 > num2) then
                                     result := num1

                                  else
                                     result := num2;
                                  max := error;
                               end;

                               begin
                                  a := 100;
                                  b := 200;
                                  (* calling a function to get max value *)
                                  ret := max(a, b);

                               end.
                               """;

        TypeCheckVisitor visitor = CheckType(program);
        Assert.True(visitor.IsError);
    }

    [Fact]
    public void IfStatementTest()
    {
        const string program = """
                               program exFunction;
                               var
                                  a, b, ret : integer;
                               begin
                                  a := 100;
                                  b := 200;
                                  if 200 then
                                      begin
                                        b := 100;
                                      end
                                      else
                                  b:=200;
                               end.
                               """;

        TypeCheckVisitor visitor = CheckType(program);
        Assert.True(visitor.IsError);
    }

    [Fact]
    public void ForStatementTest()
    {
        const string program = """
                               program exFunction;
                               var
                                  a, b, ret : integer;
                                  c : char;
                               begin

                                  for a := c to b do
                                   begin
                                   b:=b+10;
                                   end;

                               end.
                               """;

        TypeCheckVisitor visitor = CheckType(program);
        Assert.True(visitor.IsError);
    }

    [Fact]
    public void ProcedureCallTest()
    {
        const string program = """
                               program main;
                               var
                                  a, b, c,  min: integer;
                                  error:char;
                               procedure findMin(x, y, z: integer; var m: integer);
                               begin
                               end;

                               begin
                                  findmin(a, b, c,error);
                                  (* Procedure call *)
                               end.
                               """;

        TypeCheckVisitor visitor = CheckType(program);
        Assert.True(visitor.IsError);
    }


    [Fact]
    public void RecursionProcedureCallTest()
    {
        const string program = """
                               program main;
                               var a, b:integer; c:real;
                               function Test0(var a1:integer; b1:integer; c1:real):integer;
                               begin
                               test0(a1,b1,c1+0.5);
                               end;
                               function Test1(var a1:integer; b1:integer; c1:real):integer;
                               begin
                               test0(1,1,1.0);
                               end;

                               begin
                               teSt1(a,b,1.02);
                               test(a, b, c);
                               end.
                               """;

        TypeCheckVisitor visitor = CheckType(program);
        Assert.True(visitor.IsError);
    }


    [Fact]
    public void ArrayAssignIndexTest()
    {
        const string program = """
                               program main;
                               var a : array [0..10, 0..10] of integer;
                               function test(a, b : integer) : integer;
                               begin
                                   test := 1;
                               end;
                               begin
                               a[0, 1.5] := 1;
                               end.
                               """;

        TypeCheckVisitor visitor = CheckType(program);
        Assert.True(visitor.IsError);
    }

    [Fact]
    public void ArrayAssignDimensionTest()
    {
        const string program = """
                               program main;
                               var a : array [0..10, 0..10] of integer;
                               begin
                               a[0,1,3] := 1;
                               end.
                               """;

        TypeCheckVisitor visitor = CheckType(program);
        Assert.True(visitor.IsError);
    }

    [Fact]
    public void ArrayAssignTypeTest()
    {
        const string program = """
                               program main;
                               var
                               a : array [0..10, 0..10] of integer;
                               b : array [0..10, 0..20] of integer;
                               c : integer;
                               d : char;
                               begin
                               a[0,1] := c;
                               c := b[0,5];
                               a[0,1] := b;
                               end.
                               """;

        TypeCheckVisitor visitor = CheckType(program);
        Assert.True(visitor.IsError);
    }

    [Fact]
    public void ArrayCalculationTest()
    {
        const string program = """
                               program main;
                               var a: array[9..12, 3..5, 6..20] of real;
                               b: array[0..10] of integer;
                               begin
                               a[9, 4, 20] := 3.6 + b[5];
                               end.
                               """;

        CheckType(program);
    }

    [Fact]
    public void BooleanOperatorTest()
    {
        const string program = """
                               program main;
                               var flag, tag : boolean;
                               error:integer;
                               begin
                                   tag := flag or tag;
                                   flag := flag and error;
                               end.
                               """;

        TypeCheckVisitor visitor = CheckType(program);
        Assert.True(visitor.IsError);
    }

    [Fact]
    public void TrueFalseTest()
    {
        const string program = """
                               program main;
                               var a : boolean;
                               begin
                                a := true;
                                a := false;
                               end.
                               """;
        TypeCheckVisitor visitor = CheckType(program);
        Assert.False(visitor.IsError);
    }

    [Fact]
    public void NotTest()
    {
        const string program = """
                               program main;
                               var a: integer;
                               begin
                               a := 60;
                               write(not a);
                               end.
                               """;

        TypeCheckVisitor visitor = CheckType(program);
        Assert.False(visitor.IsError);
    }

    [Fact]
    public void PascalFunctionTest()
    {
        const string program = """
                               program main;
                               var a : integer;
                               begin
                               write(a);
                               read(a);
                               writeln(a);
                               end.
                               """;

        TypeCheckVisitor visitor = CheckType(program);
        Assert.False(visitor.IsError);
    }

    [Fact]
    public void FunctionCalculateTest()
    {
        const string program = """
                               program main;
                               var a : integer;
                               function test : integer;
                               begin
                                test := 1;
                               end;
                               begin
                               a := a + test;
                               end.
                               """;

        TypeCheckVisitor visitor = CheckType(program);
        Assert.False(visitor.IsError);
    }

    [Fact]
    public void FunctionParameterCalculationTest()
    {
        const string program = """
                               program main;
                               var a : integer;
                               function test (p : integer) : integer;
                               begin
                               test := p;
                               end;
                               begin
                               a := 1 + test(1);
                               end.
                               """;

        TypeCheckVisitor visitor = CheckType(program);
        Assert.False(visitor.IsError);
    }

    private TypeCheckVisitor CheckType(string program)
    {
        ProgramStruct root = CompilerHelpers.Analyse(program);
        TypeCheckVisitor visitor = new(_logger);
        SyntaxTreeTraveller traveller = new();

        traveller.Travel(root, visitor);

        return visitor;
    }
}
