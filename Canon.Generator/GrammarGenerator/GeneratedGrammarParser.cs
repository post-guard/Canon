using System.Text;
using Canon.Core.Abstractions;
using Canon.Core.GrammarParser;

namespace Canon.Generator.GrammarGenerator;

public class GeneratedGrammarParser(
    Dictionary<string, GeneratedTransformer> transformers,
    string beginState,
    NonTerminator begin) : IGrammarParser
{
    public ITransformer BeginTransformer => transformers[beginState];

    public NonTerminator Begin => begin;

    public string GenerateCode(string namespaceValue)
    {
        StringBuilder builder = new();
        builder.Append("#nullable enable\n");
        builder.Append("using Canon.Core.Abstractions;\n");
        builder.Append("using Canon.Core.GrammarParser;\n");
        builder.Append("using Canon.Core.Enums;\n");
        builder.Append($"namespace {namespaceValue};\n");

        builder.Append('\n');
        builder.Append("""
                       public class GeneratedTransformer : ITransformer
                       {
                           private IDictionary<TerminatorBase, string> _shiftPointers;

                           public string Name { get; }

                           public IDictionary<Terminator, ReduceInformation> ReduceTable { get; }

                           public IDictionary<TerminatorBase, ITransformer> ShiftTable { get; }

                           public GeneratedTransformer(Dictionary<TerminatorBase, string> shiftTable,
                               Dictionary<Terminator, ReduceInformation> reduceTable, string name)
                           {
                               ReduceTable = reduceTable;
                               ShiftTable = new Dictionary<TerminatorBase, ITransformer>();
                               _shiftPointers = shiftTable;
                               Name = name;
                           }

                           public GeneratedTransformer()
                           {
                               ReduceTable = new Dictionary<Terminator, ReduceInformation>();
                               ShiftTable = new Dictionary<TerminatorBase, ITransformer>();
                               _shiftPointers = new Dictionary<TerminatorBase, string>();
                               Name = Guid.NewGuid().ToString();
                           }

                           public void ConstructShiftTable(Dictionary<string, GeneratedTransformer> transformers)
                           {
                               foreach (KeyValuePair<TerminatorBase,string> pair in _shiftPointers)
                               {
                                   ShiftTable.Add(pair.Key, transformers[pair.Value]);
                               }
                           }

                           public override bool Equals(object? obj)
                           {
                               if (obj is not GeneratedTransformer other)
                               {
                                   return false;
                               }

                               return Name == other.Name;
                           }

                           public override int GetHashCode() => Name.GetHashCode();
                       }
                       """);
        builder.Append('\n');

        builder.Append("public class GeneratedGrammarParser : IGrammarParser\n")
            .Append("{\n");

        builder.Append("\tprivate static readonly Dictionary<string, GeneratedTransformer> s_transformers = new()\n")
            .Append("\t{\n");

        foreach (KeyValuePair<string, GeneratedTransformer> pair in transformers)
        {
            builder.Append($"\t\t{{ \"{pair.Key}\", {pair.Value.GenerateCode()} }},\n");
        }

        builder.Append("\t};\n");
        builder.Append("\n");

        builder.Append("""
                           private GeneratedGrammarParser()
                           {
                               foreach(GeneratedTransformer transformer in s_transformers.Values)
                               {
                                   transformer.ConstructShiftTable(s_transformers);
                               }
                           }

                           private static GeneratedGrammarParser s_instance = new GeneratedGrammarParser();

                           public static GeneratedGrammarParser Instance => s_instance;

                       """);
        builder.Append("\n");

        builder.Append('\t').Append("public ITransformer BeginTransformer => ")
            .Append($"s_transformers[\"{beginState}\"];\n");

        builder.Append('\t').Append("public NonTerminator Begin => ")
            .Append(begin.GenerateCode()).Append(";\n");

        builder.Append("}\n");
        return builder.ToString();
    }
}
