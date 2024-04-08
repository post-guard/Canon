using System.CommandLine;
using Canon.Generator.Extensions;

RootCommand rootCommand = new("Canon Compiler Source Generator");

rootCommand.AddGenerateCommand();

await rootCommand.InvokeAsync(args);
