using Canon.Core.SyntaxNodes;
using Canon.Tests.Utils;

namespace Canon.Tests.GrammarParserTests;

public class PascalGrammarTests
{
    [Fact]
    public void DoNothingTest()
    {
        const string program = """
                               program DoNothing;
                               begin
                               end.
                               """;

        ProgramStruct root = CompilerHelpers.Analyse(program);
        Assert.Equal("DoNothing", root.Head.ProgramName.LiteralValue);
        Assert.Equal(15, root.Count());
    }

    [Fact]
    public void AddTest()
    {
        const string program = """
                               program Add;
                               var a : Integer;
                               begin
                               a := 1 + 1
                               end.
                               """;

        ProgramStruct root = CompilerHelpers.Analyse(program);
        Assert.Equal("Add", root.Head.ProgramName.LiteralValue);
    }

    [Fact]
    public void WriteLnTest()
    {
        const string program = """
                               program exFunction;
                               const str = 'a';
                               var a, b : Integer;
                               begin
                               writeln( str, ret );
                               end.
                               """;

        ProgramStruct root = CompilerHelpers.Analyse(program);
        Assert.Equal("exFunction", root.Head.ProgramName.LiteralValue);
    }

    [Fact]
    public void CharacterTest()
    {
        const string program = """
                               program varTest;
                               var a : integer;
                               begin
                               a := 9 div 1;
                               end.
                               """;

        ProgramStruct root = CompilerHelpers.Analyse(program);
        Assert.Equal("vartest", root.Head.ProgramName.IdentifierName);
    }

    [Fact]
    public void ArrayTest()
    {
        const string program = """
                               program arrayTest;
                               var a : array [0..10] of integer;
                               begin
                               a[0] := 1;
                               end.
                               """;

        CompilerHelpers.Analyse(program);
    }

    [Fact]
    public void MultiplyArrayTest()
    {
        const string program = """
                               program arrayTest;
                               var a : array [10..100, 0..10] of integer;
                               begin
                               a[10,0] := 1;
                               end.
                               """;

        CompilerHelpers.Analyse(program);
    }

    [Fact]
    public void ProcedureTest()
    {
        const string program = """
                               program main;
                               procedure test;
                               begin
                               end;
                               begin
                               end.
                               """;

        CompilerHelpers.Analyse(program);
    }

    [Fact]
    public void FunctionTest()
    {
        const string program = """
                               program main;
                               function test(a, b : integer) : integer;
                               begin
                               test := 1;
                               end;
                               begin
                               end.
                               """;

        CompilerHelpers.Analyse(program);
    }

    [Fact]
    public void ForLoopTest()
    {
        const string program = """
                               program main;
                               var i : integer;
                               begin
                               for i := 1 to 100 do
                               begin
                               doSomething(i);
                               end;
                               end.
                               """;

        CompilerHelpers.Analyse(program);
    }

    [Fact]
    public void IfConditionTest()
    {
        const string program = """
                               program main;
                               begin
                               if 1 = 2 then
                               begin
                               test1;
                               test2 := a;
                               end
                               else
                               begin
                               doSomething;
                               end;
                               end.
                               """;

        CompilerHelpers.Analyse(program);
    }

    [Fact]
    public void ProcedureCallTest()
    {
        const string program = """
                               program main;
                               var a : integer;
                               function test : integer;
                               begin
                               end;
                               begin
                               test;
                               a := test;
                               test();
                               a := test();
                               end.
                               """;

        CompilerHelpers.Analyse(program);
    }

    [Fact]
    public void ProcedureDefinitionTest()
    {
        const string program = """
                               program main;
                               procedure test();
                               begin
                               end;
                               begin
                               test();
                               end.
                               """;

        CompilerHelpers.Analyse(program);
    }

    [Fact]
    public void FactorTest()
    {
        const string program = """
                               program main;
                               var a : integer;
                               begin
                               a := 1 + +1;
                               a := 1 - -1;
                               a := 1 + -1;
                               a := 1 - +1;
                               end.
                               """;

        CompilerHelpers.Analyse(program);
    }

    [Fact]
    public void TrueFalseTest()
    {
        const string program = """
                               program main;
                               const a = true; b = false;
                               var c, d : boolean;
                               begin
                               c := true;
                               d := false;
                               end.
                               """;

        CompilerHelpers.Analyse(program);
    }

    [Fact]
    public void ProcedureAndVariableTest()
    {
        const string program = """
                               program main;
                               begin
                               test
                               end.
                               """;

        CompilerHelpers.Analyse(program);
    }

    [Fact]
    public void WhileLoopTest()
    {
        const string program = """
                               program main;
                               var i : integer;
                               begin
                               while i < 10 do
                               i := i + 1;
                               end.
                               """;

        CompilerHelpers.Analyse(program);
    }
}
