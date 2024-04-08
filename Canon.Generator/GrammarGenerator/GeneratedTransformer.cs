using System.Text;
using Canon.Core.Abstractions;
using Canon.Core.GrammarParser;

namespace Canon.Generator.GrammarGenerator;

public class GeneratedTransformer : ITransformer
{
    private readonly IDictionary<TerminatorBase, string> _shiftPointers;

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

    public string GenerateCode()
    {
        StringBuilder builder = new();

        builder.Append("new GeneratedTransformer(new Dictionary<TerminatorBase, string>").Append("{");
        foreach (KeyValuePair<TerminatorBase, ITransformer> pair in ShiftTable)
        {
            builder.Append($" {{ {pair.Key.GenerateCode()}, \"{pair.Value.Name}\"}},");
        }

        builder.Append("}, ");

        builder.Append("new Dictionary<Terminator, ReduceInformation>{");

        foreach (KeyValuePair<Terminator,ReduceInformation> pair in ReduceTable)
        {
            builder.Append($" {{ {pair.Key.GenerateCode()}, {pair.Value.GenerateCode()}}},");
        }

        builder.Append($" }}, \"{Name}\")");

        return builder.ToString();
    }
}
