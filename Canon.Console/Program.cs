using System.CommandLine;
using Canon.Console.Extensions;

RootCommand rootCommand = new("Canon Pascal Compiler (PASCC).");
rootCommand.SetCompile();

await rootCommand.InvokeAsync(args);
