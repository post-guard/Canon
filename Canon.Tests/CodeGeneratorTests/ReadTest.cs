using Canon.Core.SemanticParser;
using Canon.Core.SyntaxNodes;
using Canon.Tests.Utils;
using Xunit.Abstractions;

namespace Canon.Tests.CodeGeneratorTests;

public class ReadTest
{
    private readonly ITestOutputHelper _output;

    public ReadTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void SimpleReadTest()
    {
        const string program = """
                               program main;
                               var a, b:integer;
                               begin
                               read(a);
                               write(b + 1);
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
}
