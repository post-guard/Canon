using System.Text;
using Canon.Core.Abstractions;
using Canon.Core.Enums;
using Canon.Core.GrammarParser;

namespace Canon.Generator.GrammarGenerator;

public class GenerateCommand
{
    private readonly GrammarBuilder _builder = new()
    {
        Generators = PascalGrammar.Grammar, Begin = new NonTerminator(NonTerminatorType.StartNonTerminator)
    };

    private readonly GeneratedGrammarParser _parser;

    public GenerateCommand()
    {
        Grammar grammar = _builder.Build();
        _parser = grammar.ToGeneratedGrammarParser();
    }

    public async Task GenerateCode(Stream output, string namespaceValue)
    {
        string code = _parser.GenerateCode(namespaceValue);
        byte[] bytes = Encoding.UTF8.GetBytes(code);
        await output.WriteAsync(bytes);
    }
}
