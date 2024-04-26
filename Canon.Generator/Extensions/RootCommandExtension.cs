using System.CommandLine;
using Canon.Generator.GrammarGenerator;

namespace Canon.Generator.Extensions;

public static class RootCommandExtension
{
    public static void AddGrammarCommand(this RootCommand rootCommand)
    {
        Command generateCommand = new("grammar", "Generate grammar parser source files.");

        Argument<string> filenameArgument = new(name: "filename",
            description: "determines the generated file name.",
            getDefaultValue: () => "Canon.g.cs");
        generateCommand.AddArgument(filenameArgument);

        Option<string> namespaceOption = new(name: "--namespace",
            description: "determines the namespace of generated code.",
            getDefaultValue: () => "Canon.Generator.GrammarGenerator");
        generateCommand.AddOption(namespaceOption);

        generateCommand.SetHandler(async (context) =>
        {
            string filename = context.ParseResult.GetValueForArgument(filenameArgument);
            FileInfo generatedFile = new(Path.Combine(Environment.CurrentDirectory, filename));
            if (generatedFile.Exists)
            {
                generatedFile.Delete();
            }

            await using FileStream stream = generatedFile.OpenWrite();
            GenerateCommand command = new();

            string namespaceValue = context.ParseResult.GetValueForOption(namespaceOption) ?? "Canon.Generator.GrammarGenerator";
            await command.GenerateCode(stream, namespaceValue);
        });

        rootCommand.AddCommand(generateCommand);
    }

    public static void AddSyntaxVisitorCommand(this RootCommand rootCommand)
    {
        Command syntaxCommand = new("syntax", "Generate syntax visitor source code.");

        syntaxCommand.SetHandler(async () =>
        {
            await SyntaxVisitorGenerator.SyntaxVisitorGenerator.Generate();
        });

        rootCommand.AddCommand(syntaxCommand);
    }
}
