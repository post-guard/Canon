using Canon.Core.LexicalParser;

namespace Canon.Core.GrammarParser;

public class Grammar
{
    public required NonTerminator Begin { get; init; }

    public required LrState BeginState { get; init; }

    public SyntaxNode Analyse(IEnumerable<SemanticToken> tokens)
    {
        Stack<AnalyseState> stack = [];
        stack.Push(new AnalyseState(BeginState, new SyntaxNode(SemanticToken.End)));

        using IEnumerator<SemanticToken> enumerator = tokens.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            throw new InvalidOperationException("Input token list is empty");
        }

        while (true)
        {
            AnalyseState top = stack.Peek();

            // 尝试进行移进
            bool acceptFlag = false, reduceFlag = false;
            foreach (Expression e in top.State.Expressions)
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
                        SyntaxNode newNode = new(e.Left.Type);

                        for (int i = 0; i < e.Right.Count; i++)
                        {
                            newNode.Children.Add(stack.Pop().Node);
                        }

                        stack.Push(new AnalyseState(stack.Peek().State.Transformer[e.Left],
                            newNode));
                    }
                    break;
                }

                if (e.Right.Count == 0 && e.LookAhead == enumerator.Current)
                {
                    // 考虑空产生式的归约
                    // 显然空产生式是不能accept的
                    reduceFlag = true;
                    SyntaxNode newNode = new(e.Left.Type);

                    stack.Push(new AnalyseState(stack.Peek().State.Transformer[e.Left],
                        newNode));
                }
            }

            if (acceptFlag)
            {
                // 接受文法 退出循环
                return top.Node;
            }

            if (reduceFlag)
            {
                // 归约
                continue;
            }

            // 尝试进行移进
            if (top.State.Transformer.TryGetValue(enumerator.Current, out LrState? next))
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

    private record AnalyseState(LrState State, SyntaxNode Node);
}
