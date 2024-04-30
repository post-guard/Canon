using Canon.Core.SemanticParser;
using Canon.Core.SyntaxNodes;
using Canon.Tests.Utils;
using Xunit.Abstractions;

namespace Canon.Tests.CodeGeneratorTests;

public class DeclarationTests
{
    private readonly ITestOutputHelper _output;

    public DeclarationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ConstDeclarationTest()
    {
        const string program = """
                               program main;
                               const a = 'a'; b = 200; c = 3.14; d = 'm';
                               begin
                               end.
                               """;

        ProgramStruct root = CompilerHelpers.Analyse(program);
        SyntaxTreeTraveller traveller = new();
        CCodeGenerateVisitor visitor = new();
        traveller.Travel(root, visitor);

        string result = visitor.Builder.Build();
        _output.WriteLine(result);
        Assert.Equal("#include <stdbool.h>\n#include <stdio.h>\nconst char a = 'a';\nconst " +
                     "int b = 200;\nconst double c = 3.14;\nconst char d = 'm';\nint main()\n{\n;\n\nreturn 0;\n}\n",
            visitor.Builder.Build());
    }

    [Fact]
    public void VarDeclarationTest()
    {
        const string program = """
                               program main;
                               var a, b, c:array[3..6, 4..999, 0..7, 8..80] of real;
                                    d, e, f:integer;
                                    g, h:array [6..8] of boolean;
                                    i, j:char;
                                    m, n:integer;
                               begin
                               end.
                               """;

        ProgramStruct root = CompilerHelpers.Analyse(program);
        SyntaxTreeTraveller traveller = new();
        CCodeGenerateVisitor visitor = new();
        traveller.Travel(root, visitor);

        string result = visitor.Builder.Build();
        _output.WriteLine(result);
        Assert.Equal("#include <stdbool.h>\n#include <stdio.h>\ndouble c[4][996][8][73]," +
                     " b[4][996][8][73], a[4][996][8][73];\nint f, e, d;\nbool h[3], g[3];\nchar j, i;" +
                     "\nint n, m;\nint main()\n{\n;\n\nreturn 0;\n}\n", visitor.Builder.Build());
    }
}
