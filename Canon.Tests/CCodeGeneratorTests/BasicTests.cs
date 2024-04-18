using Canon.Core.Abstractions;
using Canon.Core.CodeGenerators;
using Canon.Core.LexicalParser;
using Canon.Core.SyntaxNodes;
using Canon.Tests.GeneratedParserTests;
using Canon.Tests.Utils;

namespace Canon.Tests.CCodeGeneratorTests;

public class BasicTests
{
    private readonly IGrammarParser _parser = GeneratedGrammarParser.Instance;
    private readonly ILexer _lexer = new Lexer();

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

        Assert.Equal("#include <PascalCoreLib.h>", builder.Build());
    }
}
