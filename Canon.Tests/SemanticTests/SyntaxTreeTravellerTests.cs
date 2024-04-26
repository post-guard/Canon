using Canon.Core.Abstractions;
using Canon.Core.GrammarParser;
using Canon.Core.LexicalParser;
using Canon.Core.SemanticParser;
using Canon.Core.SyntaxNodes;
using Canon.Tests.Utils;

namespace Canon.Tests.SemanticTests;

public class SyntaxTreeTravellerTests
{
    private readonly ILexer _lexer = new Lexer();
    private readonly IGrammarParser _grammarParser = GeneratedGrammarParser.Instance;
    private readonly SyntaxTreeTraveller _traveller = new();

    [Fact]
    public void TravelTest()
    {
        const string program = """
                               program main;
                               begin
                               end.
                               """;

        SampleSyntaxTreeVisitor visitor = new();
        IEnumerable<SemanticToken> tokens = _lexer.Tokenize(new StringSourceReader(program));
        ProgramStruct root = _grammarParser.Analyse(tokens);

        _traveller.Travel(root, visitor);

        List<string> result =
        [
            "ProgramStruct",
            "ProgramHead",
            "ProgramHead",
            "ProgramBody",
            "SubprogramDeclarations",
            "SubprogramDeclarations",
            "CompoundStatement",
            "StatementList",
            "Statement",
            "Statement",
            "StatementList",
            "CompoundStatement",
            "ProgramBody",
            "ProgramStruct"
        ];

        string[] actual = visitor.ToString().Split('\n');

        foreach ((string line, uint index) in result.WithIndex())
        {
            Assert.Equal(line, actual[(int)index]);
        }
    }
}
