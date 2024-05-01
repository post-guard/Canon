using Canon.Core.Abstractions;
using Canon.Core.Enums;
using Canon.Core.Exceptions;

namespace Canon.Core.GrammarParser;

/// <summary>
/// 通过LR分析方法建立的语法
/// </summary>
public class Grammar
{
    /// <summary>
    /// 起始符
    /// </summary>
    public required NonTerminator Begin { get; init; }

    /// <summary>
    /// 语法中的DFA
    /// </summary>
    public required HashSet<LrState> Automation { get; init; }

    /// <summary>
    /// 起始状态
    /// </summary>
    public required LrState BeginState { get; init; }

    public IGrammarParser ToGrammarParser()
    {
        Dictionary<LrState, Transformer> transformers = [];

        foreach (LrState state in Automation)
        {
            ITransformer transformer;
            if (transformers.TryGetValue(state, out Transformer? oldTransformer))
            {
                transformer = oldTransformer;
            }
            else
            {
                Transformer newTransformer = new();
                transformers.Add(state, newTransformer);
                transformer = newTransformer;
            }

            // 生成归约的迁移表
            foreach (Expression expression in state.Expressions)
            {
                if (expression.Pos == expression.Right.Count)
                {
                    if (transformer.ShiftTable.ContainsKey(expression.LookAhead))
                    {
                        throw new ReduceAndShiftConflictException();
                    }

                    if (!transformer.ReduceTable.TryAdd(expression.LookAhead,
                            new ReduceInformation(expression.Right.Count, expression.Left)))
                    {
                        // 发生归约-归约冲突
                        throw new ReduceConflictException(state, expression.LookAhead, expression.Left,
                            transformer.ReduceTable[expression.LookAhead].Left);
                    }
                }
            }

            // 生成移进的迁移表
            foreach (KeyValuePair<TerminatorBase, LrState> pair in state.Transformer)
            {
                ITransformer targetTransformer;
                if (transformers.TryGetValue(pair.Value, out Transformer? oldTransformer2))
                {
                    targetTransformer = oldTransformer2;
                }
                else
                {
                    Transformer newTransformer = new();
                    transformers.Add(pair.Value, newTransformer);
                    targetTransformer = newTransformer;
                }

                // 检测移进-归约冲突
                if (pair.Key.IsTerminated)
                {
                    Terminator terminator = (Terminator)pair.Key;
                    // hack 对于ElsePart的移进-归约冲突
                    if (terminator != new Terminator(KeywordType.Else) && transformer.ReduceTable.ContainsKey(terminator))
                    {
                        throw new ReduceAndShiftConflictException();
                    }
                }

                transformer.ShiftTable.Add(pair.Key, targetTransformer);
            }
        }

        return new GrammarParser(transformers[BeginState], Begin);
    }

    private class GrammarParser(ITransformer beginTransformer, NonTerminator begin) : IGrammarParser
    {
        public ITransformer BeginTransformer { get; } = beginTransformer;
        public NonTerminator Begin { get; } = begin;
    }

    private class Transformer : ITransformer
    {
        public string Name => string.Empty;

        public IDictionary<TerminatorBase, ITransformer> ShiftTable { get; }
            = new Dictionary<TerminatorBase, ITransformer>();

        public IDictionary<Terminator, ReduceInformation> ReduceTable { get; }
            = new Dictionary<Terminator, ReduceInformation>();
    }
}
