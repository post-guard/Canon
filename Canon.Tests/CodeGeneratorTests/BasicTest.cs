using Canon.Core.SemanticParser;
using Canon.Core.SyntaxNodes;
using Canon.Tests.Utils;
using Xunit.Abstractions;

namespace Canon.Tests.CodeGeneratorTests;

public class BasicTest
{
    private readonly ITestOutputHelper _output;

    public BasicTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ProgramStructTest()
    {
        const string program = """
                               program main;
                               begin
                               end.
                               """;

        ProgramStruct root = CompilerHelpers.Analyse(program);
        SyntaxTreeTraveller traveller = new();
        CCodeGenerateVisitor visitor = new();
        traveller.Travel(root, visitor);

        string result = visitor.Builder.Build();
        _output.WriteLine(result);
        Assert.Equal("#include <stdbool.h>\n#include <stdio.h>\nint main()\n{\n;\n\nreturn 0;\n}\n",
            visitor.Builder.Build());
    }
}
