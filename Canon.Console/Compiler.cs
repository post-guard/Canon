namespace Canon.Console;

public class Compiler
{
    private readonly Dictionary<string, string> _options = [];
    private readonly FileInfo _sourceFile;

    public Compiler(string[] args)
    {
        if (args.Length < 2)
        {
            throw new ArgumentException("Please provide pascal file name with option '-i' at least!");
        }

        for (int i = 0; i < args.Length; i += 2)
        {
            _options.Add(args[i], args[i + 1]);
        }

        if (!_options.TryGetValue("-i", out string? value))
        {
            throw new ArgumentException("Please provide pascal file name with option '-i' at least!");
        }

        _sourceFile = new FileInfo(Path.Combine(Environment.CurrentDirectory, value));

        if (!_sourceFile.Exists)
        {
            throw new InvalidOperationException("Source file not exists!");
        }
    }

    public async Task Compile()
    {
        using StreamReader source = _sourceFile.OpenText();
        await using StreamWriter output = GetOutputFile().CreateText();

        await output.WriteAsync("""
                                #include <stdio.h>
                                int main()
                                {
                                    printf("%d", 3);
                                    return 0;
                                }
                                """);
    }

    private FileInfo GetOutputFile()
    {
        return new FileInfo(Path.Combine(_sourceFile.DirectoryName!,
            Path.GetFileNameWithoutExtension(_sourceFile.Name)) + ".c");
    }
}
