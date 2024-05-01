using Canon.Core.GrammarParser;

namespace Canon.Core.Exceptions;

public class ReduceConflictException(LrState originState, Terminator lookAhead, NonTerminator left1, NonTerminator left2)
    : Exception
{
    public LrState OriginState { get; } = originState;

    public Terminator LookAhead { get; } = lookAhead;

    public NonTerminator Left1 { get; } = left1;

    public NonTerminator Left2 { get; } = left2;

    public override string Message => "Reduce Conflict!";
}
