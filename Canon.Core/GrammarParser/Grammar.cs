namespace Canon.Core.GrammarParser;

public class Grammar
{
    public required NonTerminator Begin { get; init; }

    public required LrState BeginState { get; init; }
}
