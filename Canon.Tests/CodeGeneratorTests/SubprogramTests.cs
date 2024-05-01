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
        Assert.Equal("""
                     #include <stdbool.h>
                     #include <stdio.h>
                     bool b, a;
                     int func1(int* a, int b, double c)
                     {
                     int func1;
                     {
                     (*a) = b + c;
                     func1 = (*a) * 3;
                     ;
                     }
                     return func1;
                     }
                     char func2(bool* a, bool* b, char c[][6])
                     {
                     char func2;
                     {
                     (*a) = (*b) && (~(*b));
                     func2 = c[5-0][8-3];
                     ;
                     }
                     return func2;
                     }
                     int main()
                     {
                     ;

                     return 0;
                     }

                     """, visitor.Builder.Build());
    }
}
