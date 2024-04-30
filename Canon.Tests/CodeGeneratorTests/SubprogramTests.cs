using Canon.Core.SemanticParser;
using Canon.Core.SyntaxNodes;
using Canon.Tests.Utils;
using Xunit.Abstractions;

namespace Canon.Tests.CodeGeneratorTests;

public class SubprogramTests
{
    private readonly ITestOutputHelper _output;

    public SubprogramTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ProcedureDeclarationTest()
    {
        const string program = """
                               program main;
                               const PI = 3.1415;
                               procedure test1;
                               var ch:char;
                               begin
                               end;
                               procedure test2;
                               var i, j:integer;
                               begin
                               end;
                               begin
                               end.
                               """;

        ProgramStruct root = CompilerHelpers.Analyse(program);
        SyntaxTreeTraveller traveller = new();
        CCodeGenerateVisitor visitor = new();
        traveller.Travel(root, visitor);

        string result = visitor.Builder.Build();
        _output.WriteLine(result);
        Assert.Equal("#include <stdbool.h>\n#include <stdio.h>\nconst double pi = 3.1415;\n" +
                     "void test1()\n{\nchar ch;\n{\n;\n}\n\n}\n" +
                     "void test2()\n{\nint j, i;\n{\n;\n}\n\n}\n" +
                     "int main()\n{\n;\n\nreturn 0;\n}\n", visitor.Builder.Build());
    }

    [Fact]
    public void FunctionDeclarationTest()
    {
        const string program = """
                               program main;
                               var a, b: boolean;
                               function func1(var a:integer; b:integer; c:real):integer;
                               begin
                                a := b + c;
                                func1 := a * 3;
                               end;
                               function func2(var a, b:boolean; c: array[0..6,3..8] of char):char;
                               begin
                                a := b and not b;
                                func2 := c[5,8];
                               end;
                               begin
                               end.
                               """;

        ProgramStruct root = CompilerHelpers.Analyse(program);
        SyntaxTreeTraveller traveller = new();
        CCodeGenerateVisitor visitor = new();
        traveller.Travel(root, visitor);

        string result = visitor.Builder.Build();
        _output.WriteLine(result);
        Assert.Equal("#include <stdbool.h>\n#include <stdio.h>\nbool b, a;" +
                     "\nint func1(int* a, int b, double c)\n{\nint func1;\n" +
                     "{\n(*a) = b + c;\nfunc1 = (*a) * 3;\n;\n}\nreturn func1;\n}\n" +
                     "char func2(bool* a, bool* b, char c[][6])\n{\nchar func2;\n" +
                     "{\n(*a) = (*b) && (!(*b));\nfunc2 = c[5-0][8-3];\n;\n}\nreturn func2;\n}" +
                     "\nint main()\n{\n;\n\nreturn 0;\n}\n", visitor.Builder.Build());
    }


}
