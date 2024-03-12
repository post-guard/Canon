namespace Canon.Core.GrammarParser;

public class GrammarBuilder
{
    /// <summary>
    /// 指定文法的生成式
    /// </summary>
    public Dictionary<NonTerminator, List<List<TerminatorBase>>> Generators { get; init; } = [];

    /// <summary>
    /// 文法的起始符
    /// </summary>
    public required NonTerminator Begin { get; init; }

    /// <summary>
    /// 文法中所有非终结符的First集合
    /// </summary>
    public Dictionary<NonTerminator, HashSet<Terminator>> FirstSet { get; } = [];

    public HashSet<LrState> Automation { get; } = [];

    /// <summary>
    /// 构建文法中所有非终结符的First集合
    /// </summary>
    private void BuildFirstSet()
    {
        bool changed = true;

        while (changed)
        {
            changed = false;

            foreach (KeyValuePair<NonTerminator, List<List<TerminatorBase>>> pair in Generators)
            {
                foreach (List<TerminatorBase> expression in pair.Value)
                {
                    // TODO: 对于空产生式直接跳过处理是正确的吗
                    TerminatorBase? expressionHead = expression.FirstOrDefault();
                    if (expressionHead is null)
                    {
                        continue;
                    }


                    if (expressionHead.IsTerminated)
                    {
                        // 产生式的第一个字符是终结符
                        // 将这个终结符加入该非终结符的First集合
                        Terminator terminator = (Terminator)expressionHead;

                        if (FirstSet.TryAdd(pair.Key, [terminator]))
                        {
                            changed = true;
                        }
                        else
                        {
                            if (FirstSet[pair.Key].Add(terminator))
                            {
                                changed = true;
                            }
                        }
                    }
                    else
                    {
                        NonTerminator nonTerminator = (NonTerminator)expressionHead;
                        // 产生式的第一个字符是非终结符
                        // 将该非终结符的结果合并到该
                        if (FirstSet.TryGetValue(nonTerminator, out HashSet<Terminator>? value))
                        {
                            foreach (Terminator first in value)
                            {
                                if (FirstSet.TryAdd(pair.Key, [first]))
                                {
                                    changed = true;
                                }
                                else
                                {
                                    if (FirstSet[pair.Key].Add(first))
                                    {
                                        changed = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 计算指定语句的First集合
    /// 需要用到非终结符的First集合
    /// </summary>
    /// <param name="expression">需要计算的语句</param>
    /// <returns>指定语句的First集合</returns>
    private HashSet<Terminator> CalculateFirstSetOfExpression(List<TerminatorBase> expression)
    {
        HashSet<Terminator> result = [];

        TerminatorBase? expressionHead = expression.FirstOrDefault();
        if (expressionHead is null)
        {
            return result;
        }

        if (expressionHead.IsTerminated)
        {
            // 指定表达式开头是终结符
            Terminator terminator = (Terminator)expressionHead;

            result.Add(terminator);
        }
        else
        {
            // 指定表达式开头是非终结符
            // 将该非终结符的FirstSet加入进来
            NonTerminator nonTerminator = (NonTerminator)expressionHead;

            if (!FirstSet.TryGetValue(nonTerminator, out HashSet<Terminator>? firstSet))
            {
                throw new InvalidOperationException($"Failed to get first set for {nonTerminator}");
            }

            foreach (Terminator terminator in firstSet)
            {
                // 如果First中包含空字符串
                // 递归获得该字符之后的表达式的FirstSet
                if (terminator == Terminator.EmptyTerminator)
                {
                    result.UnionWith(CalculateFirstSetOfExpression(expression[1..]));
                }
                else
                {
                    result.Add(terminator);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// 计算指定表达式的项目集规范族闭包
    /// </summary>
    /// <param name="expression">指定的表达式</param>
    /// <returns>指定表达式的项目集规范族闭包</returns>
    private HashSet<Expression> CalculateClosure(Expression expression)
    {
        HashSet<Expression> closure = [expression];

        bool changed = true;
        while (changed)
        {
            changed = false;

            // 不能在foreach过程中修改集合
            // 因此需要在遍历完成之后添加
            List<Expression> addedExpressions = [];

            foreach (Expression e in closure)
            {
                if (e.Pos >= e.Right.Count)
                {
                    // 已经移进到达句型的末尾
                    continue;
                }

                TerminatorBase next = e.Right[e.Pos];

                if (next.IsTerminated)
                {
                    continue;
                }

                NonTerminator nonTerminator = (NonTerminator)next;

                // 将当前未移进的字符和向前看字符拼接为新的向前看表达式
                List<TerminatorBase> ahead = [];
                for (int i = e.Pos + 1; i < e.Right.Count; i++)
                {
                    ahead.Add(e.Right[i]);
                }
                ahead.Add(e.LookAhead);

                HashSet<Terminator> lookAheadSet = CalculateFirstSetOfExpression(ahead);

                foreach (List<TerminatorBase> nextExpression in Generators[nonTerminator])
                {
                    foreach (Terminator lookAhead in lookAheadSet)
                    {
                        // 在新建Expression的时候就不用把空产生式放进右部里面了
                        Expression newExpression = new()
                        {
                            Left = nonTerminator, Right = IsEmptyOnly(nextExpression) ? [] : nextExpression,
                            LookAhead = lookAhead, Pos = 0
                        };

                        if (!closure.Contains(newExpression))
                        {
                            addedExpressions.Add(newExpression);
                        }
                    }
                }
            }

            foreach (Expression addedExpression in addedExpressions)
            {
                if (closure.Add(addedExpression))
                {
                    changed = true;
                }
            }
        }

        return closure;
    }

    public Grammar Build()
    {
        // 开始之前构建FirstSet
        BuildFirstSet();

        Expression begin = new()
        {
            // 这里就不考虑右部可能为空产生式的情况了
            // 毕竟有拓广文法
            Left = Begin, Right = Generators[Begin].First(), LookAhead = Terminator.EndTerminator, Pos = 0
        };

        LrState beginState = new() { Expressions = CalculateClosure(begin) };
        Automation.Add(beginState);

        bool added = true;
        while (added)
        {
            // 同样不能在foreach期间修改集合
            HashSet<LrState> addedStates = [];

            foreach (LrState state in Automation)
            {
                // 表示使用key进行移进可以生成的新LR(1)句型
                Dictionary<TerminatorBase, List<Expression>> nextExpressions = [];

                foreach (Expression e in state.Expressions)
                {
                    if (e.Pos >= e.Right.Count)
                    {
                        // 已经移进到达末尾
                        continue;
                    }

                    TerminatorBase next = e.Right[e.Pos];
                    Expression nextExpression = new()
                    {
                        Left = e.Left, Right = e.Right, LookAhead = e.LookAhead, Pos = e.Pos + 1
                    };

                    if (!nextExpressions.TryAdd(next, [nextExpression]))
                    {
                        nextExpressions[next].Add(nextExpression);
                    }
                }

                foreach (KeyValuePair<TerminatorBase,List<Expression>> pair in nextExpressions)
                {
                    // 针对每个构建项目集闭包
                    HashSet<Expression> closure = [];

                    foreach (Expression expression in pair.Value)
                    {
                        closure.UnionWith(CalculateClosure(expression));
                    }

                    LrState newState = new() { Expressions = closure };

                    if (Automation.TryGetValue(newState, out LrState? oldState))
                    {
                        // 存在这个项目集闭包
                        state.AddTransform(pair.Key, oldState);
                    }
                    else
                    {
                        // 不存在这个项目集闭包
                        // 但是需要考虑该状态在addedStates集合中的情况
                        if (addedStates.TryGetValue(newState, out LrState? addedState))
                        {
                            state.AddTransform(pair.Key, addedState);
                        }
                        else
                        {
                            state.AddTransform(pair.Key, newState);
                            addedStates.Add(newState);
                        }
                    }
                }
            }

            added = addedStates.Count != 0;
            Automation.UnionWith(addedStates);
        }

        return new Grammar { Begin = Begin, BeginState = beginState };
    }

    private static bool IsEmptyOnly(List<TerminatorBase> expression)
    {
        if (expression.Count != 1 || !expression[0].IsTerminated)
        {
            return false;
        }

        Terminator terminator = (Terminator)expression[0];

        return terminator == Terminator.EmptyTerminator;
    }
}
