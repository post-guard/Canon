using Canon.Core.Abstractions;
using Canon.Core.CodeGenerators;
using Canon.Core.GrammarParser;
using Canon.Core.LexicalParser;
using Canon.Core.SyntaxNodes;
using Canon.Tests.Utils;
using Xunit.Abstractions;

namespace Canon.Tests.CCodeGeneratorTests;

public class BasicTests
{
    private readonly IGrammarParser _parser = GeneratedGrammarParser.Instance;
    private readonly ILexer _lexer = new Lexer();
    private readonly ITestOutputHelper _outputHelper;

    public BasicTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public void ProgramStructTest()
    {
        CCodeBuilder builder = new();

        const string program = """
                               program DoNothing;
                               begin
                               end.
                               """;

        IEnumerable<SemanticToken> tokens = _lexer.Tokenize(new StringSourceReader(program));

        ProgramStruct root = _parser.Analyse(tokens);
        root.GenerateCCode(builder);

        string result = builder.Build();
        _outputHelper.WriteLine(result);
        Assert.Equal("#include <PascalCoreLib.h> int main(){statement; return 0;}", result);
    }
}
