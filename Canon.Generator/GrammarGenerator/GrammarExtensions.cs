using Canon.Core.Abstractions;
using Canon.Core.GrammarParser;

namespace Canon.Generator.GrammarGenerator;

public static class GrammarExtensions
{
    public static GeneratedGrammarParser ToGeneratedGrammarParser(this Grammar grammar)
    {
        // 建立的逻辑和原始逻辑一致
        Dictionary<LrState, GeneratedTransformer> transformers = [];

        foreach (LrState state in grammar.Automation)
        {
            GeneratedTransformer transformer;
            if (transformers.TryGetValue(state, out GeneratedTransformer? oldTransformer))
            {
                transformer = oldTransformer;
            }
            else
            {
                GeneratedTransformer generatedTransformer = new();
                transformers.Add(state, generatedTransformer);
                transformer = generatedTransformer;
            }

            foreach (Expression expression in state.Expressions)
            {
                if (expression.Pos == expression.Right.Count)
                {
                    transformer.ReduceTable.TryAdd(expression.LookAhead, new ReduceInformation(
                        expression.Right.Count, expression.Left));
                }
            }

            foreach (KeyValuePair<TerminatorBase,LrState> pair in state.Transformer)
            {
                GeneratedTransformer targetTransformer;
                if (transformers.TryGetValue(pair.Value, out GeneratedTransformer? oldTargetTransformer))
                {
                    targetTransformer = oldTargetTransformer;
                }
                else
                {
                    GeneratedTransformer newTransformer = new();
                    transformers.Add(pair.Value, newTransformer);
                    targetTransformer = newTransformer;
                }

                transformer.ShiftTable.TryAdd(pair.Key, targetTransformer);
            }
        }

        Dictionary<string, GeneratedTransformer> generatedTransformers = [];

        foreach (GeneratedTransformer transformer in transformers.Values)
        {
            generatedTransformers.Add(transformer.Name, transformer);
        }

        return new GeneratedGrammarParser(generatedTransformers, transformers[grammar.BeginState].Name, grammar.Begin);
    }
}
