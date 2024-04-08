using Canon.Core.Abstractions;
using Canon.Core.LexicalParser;
using Canon.Core.SyntaxNodes;
using Canon.Tests.GeneratedParserTests;

namespace Canon.Tests.GrammarParserTests;

public class PascalGrammarTests
{
    private readonly GrammarParserBase _parser = GeneratedGrammarParser.Instance;

    [Fact]
    public void DoNothingTest()
    {
        const string program = """
                         program DoNothing;
                         begin
                         end.
                         """;

        Lexer lexer = new(program);
        List<SemanticToken> tokens = lexer.Tokenize();
        tokens.Add(SemanticToken.End);

        ProgramStruct root = _parser.Analyse(tokens).Convert<ProgramStruct>();
        Assert.Equal("DoNothing", root.Head.ProgramName.LiteralValue);
    }
}
