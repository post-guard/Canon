using Canon.Core.Enums;
using Canon.Core.Exceptions;
using Canon.Core.GrammarParser;
using Canon.Core.LexicalParser;
using Canon.Core.SyntaxNodes;

namespace Canon.Core.Abstractions;

/// <summary>
/// 语法分析器接口
/// </summary>
public interface IGrammarParser
{
    public ITransformer BeginTransformer { get; }

    public NonTerminator Begin { get; }

    /// <summary>
    /// 分析指定的词法记号流并构建对应的语法树
    /// </summary>
    /// <param name="tokens">输入的词法记号流</param>
    /// <returns>语法树的根节点</returns>
    /// <exception cref="InvalidOperationException">语法分析错误</exception>
    public ProgramStruct Analyse(IEnumerable<SemanticToken> tokens)
    {
        Stack<AnalyseState> stack = [];
        stack.Push(new AnalyseState(BeginTransformer, SyntaxNodeBase.Create(SemanticToken.End)));

        using IEnumerator<SemanticToken> enumerator = tokens.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            throw new InvalidOperationException("Input token list is empty");
        }

        while (true)
        {
            AnalyseState top = stack.Peek();

            // 首先尝试进行移进
            if (top.State.ShiftTable.TryGetValue(enumerator.Current, out ITransformer? next))
            {
                stack.Push(new AnalyseState(next, SyntaxNodeBase.Create(enumerator.Current)));
                if (enumerator.MoveNext())
                {
                    continue;
                }
                else
                {
                    throw new GrammarException(stack.Peek().State);
                }
            }

            // 再进行归约
            if (top.State.ReduceTable.TryGetValue(enumerator.Current, out ReduceInformation? information))
            {
                if (information.Left == Begin)
                {
                    // 如果是归约到起始符
                    // 那么就直接返回不继续进行归约
                    return top.Node.Convert<ProgramStruct>();
                }

                List<SyntaxNodeBase> children = [];
                NonTerminatorType leftType = information.Left.Type;
                for (int i = 0; i < information.Length; i++)
                {
                    children.Add(stack.Pop().Node);
                }

                // 为了符合生成式的顺序而倒序
                children.Reverse();
                stack.Push(new AnalyseState(stack.Peek().State.ShiftTable[information.Left],
                    SyntaxNodeBase.Create(leftType, children)));
                continue;
            }

            throw new GrammarException(stack.Peek().State, enumerator.Current);
        }
    }

    private record AnalyseState(ITransformer State, SyntaxNodeBase Node);
}
