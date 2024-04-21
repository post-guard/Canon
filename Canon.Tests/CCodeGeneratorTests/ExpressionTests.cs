using Canon.Core.Abstractions;
using Canon.Core.CodeGenerators;
using Canon.Core.LexicalParser;
using Canon.Core.SyntaxNodes;
using Canon.Tests.GeneratedParserTests;
using Canon.Tests.Utils;
using Xunit.Abstractions;

namespace Canon.Tests.CCodeGeneratorTests;

public class ExpressionTests
{
    private readonly IGrammarParser _parser = GeneratedGrammarParser.Instance;
    private readonly ILexer _lexer = new Lexer();
    private readonly ITestOutputHelper _outputHelper;

    public ExpressionTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public void ExpressionTest1()
    {
        CCodeBuilder builder = new();

        const string program = """
                               program varTest;
                               var a, b, c, d: integer; m, n: real; k: boolean;
                               begin
                               a := 1;
                               b := a + 6 * 9 + (a + 9) * 1 - (4 + a) * 5 / 1;
                               m := b / 3;
                               d := 9 mod 1;
                               end.
                               """;

        IEnumerable<SemanticToken> tokens = _lexer.Tokenize(new StringSourceReader(program));

        ProgramStruct root = _parser.Analyse(tokens);
        root.GenerateCCode(builder);

        string result = builder.Build();
        _outputHelper.WriteLine(result);
        Assert.Equal("#include <PascalCoreLib.h> char a; int main(){statement; return 0; }", result);
    }
}
