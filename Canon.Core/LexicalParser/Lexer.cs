using Canon.Core.Abstractions;

namespace Canon.Core.LexicalParser;

public class Lexer : ILexer
{
    public IEnumerable<SemanticToken> Tokenize(ISourceReader reader)
    {
        LexerStateMachine machine = new(reader);

        return machine.Run();
    }
}
