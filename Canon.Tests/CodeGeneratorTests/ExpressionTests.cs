using Canon.Core.SemanticParser;
using Canon.Core.SyntaxNodes;
using Canon.Tests.Utils;
using Xunit.Abstractions;

namespace Canon.Tests.CodeGeneratorTests;

public class ExpressionTests
{
    private readonly ITestOutputHelper _output;

    public ExpressionTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ExpressionTest()
    {
        const string program = """
                               program main;
                               var a, b:integer; flag, tag:boolean;
                               begin
                               a := 1;
                               b := a + b * 1 / 1 - 1 div 1 - - 2;
                               tag := flag or tag;
                               end.
                               """;

        ProgramStruct root = CompilerHelpers.Analyse(program);
        SyntaxTreeTraveller traveller = new();
        CCodeGenerateVisitor visitor = new();
        traveller.Travel(root, visitor);

        string result = visitor.Builder.Build();
        _output.WriteLine(result);
        Assert.Equal("#include <stdbool.h>\n#include <stdio.h>\nint b, a;\n" +
                     "bool tag, flag;\nint main()\n{\na = 1;\nb = a + b * 1 /(double)1 - 1 / 1 - (-2);" +
                     "\ntag = flag || tag;\n;\n\nreturn 0;\n}\n", visitor.Builder.Build());
    }

    [Fact]
    public void ArrayTest()
    {
        const string program = """
                               program main;
                               var a: array[9..12, 3..5, 6..20] of real; b: array[5..10] of integer;
                               begin
                                    a[9, 4, 20] := 3.6 + b[6] - a[12, 5, 6];
                                    b[5] := 250;
                               end.
                               """;

        ProgramStruct root = CompilerHelpers.Analyse(program);
        SyntaxTreeTraveller traveller = new();
        CCodeGenerateVisitor visitor = new();
        traveller.Travel(root, visitor);

        string result = visitor.Builder.Build();
        _output.WriteLine(result);
        Assert.Equal("#include <stdbool.h>\n#include <stdio.h>\ndouble a[4][3][15];" +
                     "\nint b[6];\nint main()\n{\na[9-9][4-3][20-6] = 3.6 + b[6-5] - a[12-9][5-3][6-6];" +
                     "\nb[5-5] = 250;\n;\n\nreturn 0;\n}\n", visitor.Builder.Build());
    }

}
