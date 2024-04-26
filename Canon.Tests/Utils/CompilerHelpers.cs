using Canon.Core.Abstractions;
using Canon.Core.GrammarParser;
using Canon.Core.LexicalParser;
using Canon.Core.SyntaxNodes;

namespace Canon.Tests.Utils;

public static class CompilerHelpers
{
    public static ProgramStruct Analyse(string program)
    {
        ILexer lexer = new Lexer();
        IGrammarParser grammarParser = GeneratedGrammarParser.Instance;

        IEnumerable<SemanticToken> tokens = lexer.Tokenize(new StringSourceReader(program));
        return grammarParser.Analyse(tokens);
    }
}
