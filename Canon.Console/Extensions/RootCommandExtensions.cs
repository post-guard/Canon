using System.CommandLine;
using Canon.Console.Models;
using Canon.Console.Services;
using Canon.Core.Abstractions;
using Canon.Core.GrammarParser;
using Canon.Core.LexicalParser;
using Canon.Core.SemanticParser;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Canon.Console.Extensions;

public static class RootCommandExtensions
{
    public static void SetCompile(this RootCommand rootCommand)
    {
        Option<string> sourceFilenameOption = new("--input", "The source pascal filename")
        {
            IsRequired = true
        };
        sourceFilenameOption.AddAlias("-i");

        rootCommand.AddOption(sourceFilenameOption);

        rootCommand.SetHandler(async (context) =>
        {
            string? sourceFilename = context.ParseResult.GetValueForOption(sourceFilenameOption);
            if (sourceFilename is null)
            {
                System.Console.WriteLine("Error: please provide source filename with option '-i'.");
                return;
            }

            HostApplicationBuilder builder = Host.CreateApplicationBuilder();

            builder.Logging.ClearProviders();
            builder.Logging.Services.AddSingleton<ILoggerProvider, CompilerLoggerProvider>();

            builder.Services.AddSingleton<CompilerOption>(
                _ => new CompilerOption { SourceFilename = sourceFilename });
            builder.Services.AddTransient<ILexer, Lexer>();
            builder.Services.AddSingleton<SyntaxTreeTraveller>();
            builder.Services.AddSingleton<IGrammarParser>(_ => GeneratedGrammarParser.Instance);
            builder.Services.AddHostedService<Compiler>();

            using IHost host = builder.Build();
            await host.RunAsync();
        });
    }
}
