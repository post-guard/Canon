using Canon.Core.LexicalParser;

namespace Canon.Core.GrammarParser;

public class Grammar
{
    public required NonTerminator Begin { get; init; }

    public required LrState BeginState { get; init; }

    public void Analyse(IEnumerable<SemanticToken> tokens)
    {
        Stack<LrState> stack = [];
        stack.Push(BeginState);

        using IEnumerator<SemanticToken> enumerator = tokens.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            throw new InvalidOperationException("Input token list is empty");
        }

        while (true)
        {
            LrState top = stack.Peek();

            // 尝试进行移进
            bool acceptFlag = false, reduceFlag = false;
            foreach (Expression e in top.Expressions)
            {
                if (e.Pos == e.Right.Count && e.LookAhead == enumerator.Current)
                {
                    if (e.Left == Begin)
                    {
                        acceptFlag = true;
                    }
                    else
                    {
                        reduceFlag = true;

                        for (int i = 0; i < e.Right.Count; i++)
                        {
                            stack.Pop();
                        }

                        stack.Push(stack.Peek().Transformer[e.Left]);
                    }
                }
            }

            if (acceptFlag)
            {
                // 接受文法 退出循环
                break;
            }

            if (reduceFlag)
            {
                // 归约
                continue;
            }

            // 尝试进行移进
            if (top.Transformer.TryGetValue(enumerator.Current, out LrState? next))
            {
                stack.Push(next);
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
}
