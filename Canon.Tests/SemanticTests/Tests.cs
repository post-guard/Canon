using Canon.Core.SemanticParser;
using Canon.Core.SyntaxNodes;
using Canon.Tests.Utils;
using Xunit.Abstractions;

namespace Canon.Tests.SemanticTests;

public class Tests(ITestOutputHelper helper)
{
    [Fact]
    public void Test()
    {
        const string program = """
                               program main;

                               var
                               x, y: array[0..0] of integer;
                               a, b: integer;

                               function exgcd(a, b: integer; var x, y: integer): integer;
                               var
                               t, r: integer;
                               begin
                               if b = 0 then
                               begin
                               x := 1;
                               y := 0;
                               exgcd := a;
                               end
                               else
                               begin
                               r := exgcd(b, a mod b, x, y);
                               t := x;
                               x := y;
                               y := (t - (a div b) * y);
                               exgcd := r;
                               end;
                               end;

                               begin
                               a := 7;
                               b := 15;
                               x[0] := 1;
                               y[0] := 1;
                               exgcd(a, b, x[0], y[0]);
                               x[0] := ((x[0] mod b) + b) mod b;
                               write(x[0]);
                               end.
                               """;

        ProgramStruct root = CompilerHelpers.Analyse(program);
        SyntaxTreeTraveller traveller = new();

        CodeGeneratorVisitor visitor = new();
        traveller.Travel(root, visitor);

        helper.WriteLine(visitor.Builder.Build());
    }
}
