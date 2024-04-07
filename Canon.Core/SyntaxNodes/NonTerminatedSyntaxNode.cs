using System.Collections;
using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public abstract class NonTerminatedSyntaxNode : SyntaxNodeBase, IEnumerable<SyntaxNodeBase>
{
    public override bool IsTerminated => false;

    public abstract NonTerminatorType Type { get; }

    public required List<SyntaxNodeBase> Children { get; init; }

    public IEnumerator<SyntaxNodeBase> GetEnumerator()
    {
        yield return this;

        foreach (SyntaxNodeBase child in Children)
        {
            if (child.IsTerminated)
            {
                yield return child;
            }

            NonTerminatedSyntaxNode nonTerminatedNode = child.Convert<NonTerminatedSyntaxNode>();

            foreach (SyntaxNodeBase node in nonTerminatedNode)
            {
                yield return node;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
