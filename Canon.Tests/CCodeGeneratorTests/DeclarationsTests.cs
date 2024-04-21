using Canon.Core.Abstractions;
using Canon.Core.CodeGenerators;
using Canon.Core.LexicalParser;
using Canon.Core.SyntaxNodes;
using Canon.Tests.GeneratedParserTests;
using Canon.Tests.Utils;
using Xunit.Abstractions;

namespace Canon.Tests.CCodeGeneratorTests;

public class DeclarationsTest
{
    private readonly IGrammarParser _parser = GeneratedGrammarParser.Instance;
    private readonly ILexer _lexer = new Lexer();
    private readonly ITestOutputHelper _outputHelper;

    public DeclarationsTest(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public void VarDeclarationsTest()
    {
        CCodeBuilder builder = new();

        const string program = """
                               program varTest;
                               var a, b, c, d: integer; m, n: real; k: boolean;
                               begin
                               end.
                               """;

        IEnumerable<SemanticToken> tokens = _lexer.Tokenize(new StringSourceReader(program));

        ProgramStruct root = _parser.Analyse(tokens);
        root.GenerateCCode(builder);

        string result = builder.Build();
        _outputHelper.WriteLine(result);
        Assert.Equal("#include <PascalCoreLib.h> char a; int main(){statement; return 0; }", result);
    }

    [Fact]
    public void ConstDeclarationsTest()
    {
        CCodeBuilder builder = new();

        const string program = """
                               program varTest;
                               const a = 1; b = 2; c = 3; d = 2.5;
                               begin
                               end.
                               """;

        IEnumerable<SemanticToken> tokens = _lexer.Tokenize(new StringSourceReader(program));

        ProgramStruct root = _parser.Analyse(tokens);
        root.GenerateCCode(builder);

        string result = builder.Build();
        _outputHelper.WriteLine(result);
        Assert.Equal("#include <PascalCoreLib.h>  int main(){statement; return 0; }", result);
    }

    [Fact]
    public void ArrayDeclarationsTest()
    {
        CCodeBuilder builder = new();

        const string program = """
                               program arrayTest;
                               var a, b, c: array[1..6,5..8] of integer;
                               d: integer;
                               begin
                               a[2,3] := 10086;
                               d:=6;
                               end.
                               """;

        IEnumerable<SemanticToken> tokens = _lexer.Tokenize(new StringSourceReader(program));

        ProgramStruct root = _parser.Analyse(tokens);
        root.GenerateCCode(builder);

        string result = builder.Build();
        _outputHelper.WriteLine(result);
        Assert.Equal("#include <PascalCoreLib.h>  int main(){statement; return 0; }", result);
    }
}
