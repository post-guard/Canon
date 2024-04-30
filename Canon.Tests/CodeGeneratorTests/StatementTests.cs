using Canon.Core.SemanticParser;
using Canon.Core.SyntaxNodes;
using Canon.Tests.Utils;
using Xunit.Abstractions;

namespace Canon.Tests.CodeGeneratorTests;

public class StatementTests
{
    private readonly ITestOutputHelper _output;

    public StatementTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void IfTest()
    {
        const string program = """
                               program main;
                               var a:integer;
                               begin
                                a := 1;
                                if a = 1 then
                                a := 1
                                else
                                begin
                                if a = 2 + a then
                                a := a
                                else a := 999;
                                end;
                               end.
                               """;

        ProgramStruct root = CompilerHelpers.Analyse(program);
        SyntaxTreeTraveller traveller = new();
        CCodeGenerateVisitor visitor = new();
        traveller.Travel(root, visitor);

        string result = visitor.Builder.Build();
        _output.WriteLine(result);
        Assert.Equal("#include <stdbool.h>\n#include <stdio.h>\nint a;\nint main()\n" +
                     "{\na = 1;\nif(a == 1)\na = 1;\nelse\n{\nif(a == 2 + a)\n" +
                     "a = a;\nelse\na = 999;\n;\n;\n}\n;\n;\n;\n\nreturn 0;\n}\n", visitor.Builder.Build());
    }

    [Fact]
    public void ForLoopTest()
    {
        const string program = """
                               program main;
                               var a, b, c:integer;
                               begin
                                b := 5;
                                c := 6;
                                for a := 1 to 60 do
                                begin
                                    for b := a + c to 5 * a do
                                        begin
                                            c := 1;
                                        end;

                                end;

                               end.
                               """;

        ProgramStruct root = CompilerHelpers.Analyse(program);
        SyntaxTreeTraveller traveller = new();
        CCodeGenerateVisitor visitor = new();
        traveller.Travel(root, visitor);

        string result = visitor.Builder.Build();
        _output.WriteLine(result);
        Assert.Equal("#include <stdbool.h>\n#include <stdio.h>\n" +
                     "int c, b, a;\nint main()\n" +
                     "{\nb = 5;\nc = 6;\nfor(a = 1; a <= 60; a++){\n" +
                     "for(b = a + c; b <= 5 * a; b++)" +
                     "{\nc = 1;\n;\n}\n;\n;\n;\n}\n;\n;\n;\n\nreturn 0;\n}\n", visitor.Builder.Build());
    }

    [Fact]
    public void ProcedureCallTest()
    {
        const string program = """
                               program main;
                               var a, b:integer; c:real;

                               function test1(var a1:integer; b1:integer; c1:real):integer;
                               var i, j, k:integer;
                               begin
                               a1:= 10086;
                               b1 := 2;
                               c1 := 63;
                               test1 := test1(i, j, k);
                               end;

                               begin
                               test1(a, b, c);
                               end.
                               """;

        ProgramStruct root = CompilerHelpers.Analyse(program);
        SyntaxTreeTraveller traveller = new();
        CCodeGenerateVisitor visitor = new();
        traveller.Travel(root, visitor);

        string result = visitor.Builder.Build();
        _output.WriteLine(result);
        Assert.Equal("#include <stdbool.h>\n#include <stdio.h>\nint b, a;\ndouble c;" +
                     "\nint test1(int* a1, int b1, double c1)\n{" +
                     "\nint test1;\nint k, j, i;\n" +
                     "{\n(*a1) = 10086;\nb1 = 2;\nc1 = 63;\n" +
                     "test1 = test1(&i, j, k);\n;\n}\nreturn test1;\n}\n" +
                     "int main()\n{\ntest1(&a, b, c);\n;\n\nreturn 0;\n}\n", visitor.Builder.Build());
    }
}
