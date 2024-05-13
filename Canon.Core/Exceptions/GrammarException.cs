using System.Text;
using Canon.Core.Abstractions;
using Canon.Core.GrammarParser;
using Canon.Core.LexicalParser;

namespace Canon.Core.Exceptions;

/// <summary>
/// 语法分析中引发的异常
/// </summary>
public class GrammarException : CanonException
{
    public override string Message { get; }

    /// <summary>
    /// 语法分析错误时的分析状态
    /// </summary>
    public ITransformer CurrentState { get; }

    /// <summary>
    /// 语法分析错误时的输入符号
    /// </summary>
    public SemanticToken CurrentToken { get; }

    public GrammarException(ITransformer currentState)
    {
        CurrentState = currentState;
        CurrentToken = SemanticToken.End;

        StringBuilder builder = new();
        builder.Append("Except ");

        foreach (TerminatorBase terminatorBase in ListNextTerminators(CurrentState))
        {
            builder.Append('\'').Append(terminatorBase).Append("' ");
        }

        Message = builder.ToString();
    }

    public GrammarException(ITransformer currentState, SemanticToken currentToken)
    {
        CurrentState = currentState;
        CurrentToken = currentToken;

        StringBuilder builder = new();
        builder.Append("Expect ");

        foreach (TerminatorBase terminatorBase in ListNextTerminators(CurrentState))
        {
            builder.Append('\'').Append(terminatorBase).Append("',");
        }

        if (!CurrentToken.Equals(SemanticToken.End))
        {
            builder.Append("but '").Append(CurrentToken.LiteralValue).Append("' found.");
        }

        Message = builder.ToString();
    }

    private static List<TerminatorBase> ListNextTerminators(ITransformer state)
    {
        List<TerminatorBase> result = [];

        result.AddRange(state.ShiftTable.Keys);
        result.AddRange(state.ReduceTable.Keys);

        return result;
    }
}
