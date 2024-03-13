using Canon.Core.GrammarParser;
using Canon.Core.LexicalParser;

namespace Canon.Core.Abstractions;

/// <summary>
/// 语法分析器接口
/// </summary>
public abstract class GrammarParserBase
{
    public abstract ITransformer BeginTransformer { get; }

    public abstract NonTerminator Begin { get; }

    public SyntaxNode Analyse(IEnumerable<SemanticToken> tokens)
    {
        Stack<AnalyseState> stack = [];
        stack.Push(new AnalyseState(BeginTransformer, new SyntaxNode(SemanticToken.End)));

        using IEnumerator<SemanticToken> enumerator = tokens.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            throw new InvalidOperationException("Input token list is empty");
        }

        while (true)
        {
            AnalyseState top = stack.Peek();

            // 首先尝试进行归约
            if (top.State.ReduceTable.TryGetValue(enumerator.Current, out ReduceInformation? information))
            {
                if (information.Left == Begin)
                {
                    // 如果是归约到起始符
                    // 那么就直接返回不继续进行归约
                    return top.Node;
                }

                SyntaxNode newNode = new(information.Left.Type);
                for (int i = 0; i < information.Length; i++)
                {
                    newNode.Children.Add(stack.Pop().Node);
                }

                stack.Push(new AnalyseState(stack.Peek().State.ShiftTable[information.Left],
                    newNode));
                continue;
            }

            // 如果没有成功归约就进行移进
            if (top.State.ShiftTable.TryGetValue(enumerator.Current, out ITransformer? next))
            {
                stack.Push(new AnalyseState(next, new SyntaxNode(enumerator.Current)));
                if (enumerator.MoveNext())
                {
                    continue;
                }
                else
                {
                    throw new InvalidOperationException("Run out of token but not accept");
                }
            }

            throw new InvalidOperationException("Failed to analyse input grammar");
        }
    }

    private record AnalyseState(ITransformer State, SyntaxNode Node);
}
