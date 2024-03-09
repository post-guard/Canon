using Canon.Core;

if (args.Length < 2)
{
    Console.WriteLine("Please provide pascal file name with option '-i' at least!");
    return;
}

Dictionary<string, string> options = new();

for (int i = 0; i < args.Length; i += 2)
{
    options.Add(args[i], args[i + 1]);
}

if (!options.TryGetValue("-i", out string? value))
{
    Console.WriteLine("Please provide pascal file name with option '-i' at least!");
    return;
}

FileInfo sourceFile = new(value);
if (!sourceFile.Exists)
{
    Console.WriteLine("Target source file doesn't exist!");
    return;
}

Compiler compiler = new();
using StreamReader source = sourceFile.OpenText();
FileInfo outputFile = new(Path.Combine(sourceFile.DirectoryName!,
    Path.GetFileNameWithoutExtension(sourceFile.Name)) + ".c");
await using StreamWriter writer = outputFile.CreateText();

await writer.WriteAsync(compiler.Compile(await source.ReadToEndAsync()));
