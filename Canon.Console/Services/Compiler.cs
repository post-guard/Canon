using Canon.Console.Models;
using Canon.Core.Abstractions;
using Canon.Core.CodeGenerators;
using Canon.Core.LexicalParser;
using Canon.Core.SyntaxNodes;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Canon.Console.Services;

public class Compiler(
    CompilerOption compilerOption,
    ILexer lexer,
    IGrammarParser grammarParser,
    IHostApplicationLifetime applicationLifetime,
    ILogger<Compiler> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        IEnumerable<SemanticToken> tokens = lexer.Tokenize(await CreateSourceReader());
        ProgramStruct root = grammarParser.Analyse(tokens);

        CCodeBuilder builder = new();
        root.GenerateCCode(builder);

        await WriteToOutputFile(builder.Build());
        applicationLifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task<ISourceReader> CreateSourceReader()
    {
        if (!Path.IsPathRooted(compilerOption.SourceFilename))
        {
            compilerOption.SourceFilename = Path.Combine(Environment.CurrentDirectory, compilerOption.SourceFilename);
        }

        logger.LogDebug("Select source file: '{}'.", compilerOption.SourceFilename);

        FileInfo sourceFile = new(compilerOption.SourceFilename);
        using StreamReader reader = sourceFile.OpenText();

        return new StringSourceReader(await reader.ReadToEndAsync());
    }

    private async Task WriteToOutputFile(string compiledCode)
    {
        FileInfo outputFile = new(Path.Combine(Environment.CurrentDirectory,
            Path.GetFileNameWithoutExtension(compilerOption.SourceFilename) + ".c"));
        logger.LogDebug("Select output file: '{}'.", outputFile.Name);

        if (outputFile.Exists)
        {
            logger.LogWarning("Rewrite output file : '{}'", outputFile.Name);
        }

        await using StreamWriter writer = outputFile.CreateText();
        await writer.WriteAsync(compiledCode);
    }
}
