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
                               const str = 'result is : ';
                               var a, b : Integer;
                               begin
                               writeln( str, ret );
                               end.
                               """;

        ProgramStruct root = CompilerHelpers.Analyse(program);
        Assert.Equal("exFunction", root.Head.ProgramName.LiteralValue);
    }

    [Fact]
    public void SubprogramTest()
    {
        const string program = """
                               program main;
                               procedure test;
                               begin
                               end;
                               begin
                               end.
                               """;

        ProgramStruct root = CompilerHelpers.Analyse(program);
        Assert.Equal("main", root.Head.ProgramName.LiteralValue);
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
}
