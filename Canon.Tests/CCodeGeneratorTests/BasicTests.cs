using Canon.Core.Abstractions;
using Canon.Core.CodeGenerators;
using Canon.Core.LexicalParser;
using Canon.Core.SyntaxNodes;
using Canon.Tests.GeneratedParserTests;

namespace Canon.Tests.CCodeGeneratorTests;

public class BasicTests
{
    private readonly IGrammarParser _parser = GeneratedGrammarParser.Instance;

    [Fact]
    public void ProgramStructTest()
    {
        CCodeBuilder builder = new();

        const string program = """
                               program DoNothing;
                               begin
                               end.
                               """;

        Lexer lexer = new(program);
        List<SemanticToken> tokens = lexer.Tokenize();
        tokens.Add(SemanticToken.End);

        ProgramStruct root = _parser.Analyse(tokens);
        root.GenerateCCode(builder);

        Assert.Equal("#include <PascalCoreLib.h>", builder.Build());
    }
}
