using System.CommandLine;
using Canon.Generator.Extensions;

RootCommand rootCommand = new("Canon Compiler Source Generator");

rootCommand.AddGrammarCommand();
rootCommand.AddSyntaxVisitorCommand();

await rootCommand.InvokeAsync(args);
