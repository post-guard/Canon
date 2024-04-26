using Canon.Core.Abstractions;
using Canon.Core.SyntaxNodes;

namespace Canon.Core.SemanticParser;

public class SyntaxTreeTraveller
{
    private readonly Stack<SyntaxNodeBase> _stack = [];
    private readonly HashSet<SyntaxNodeBase> _visited = [];

    public void Travel(ProgramStruct root, SyntaxNodeVisitor visitor)
    {
        _stack.Clear();
        _visited.Clear();
        _stack.Push(root);

        while (_stack.Count != 0)
        {
            SyntaxNodeBase node = _stack.Peek();
            if (!_visited.Contains(node))
            {
                node.PreVisit(visitor);
            }

            if (node.IsTerminated)
            {
                node.PostVisit(visitor);
                _stack.Pop();
                continue;
            }

            NonTerminatedSyntaxNode nonTerminatedNode = node.Convert<NonTerminatedSyntaxNode>();

            if (nonTerminatedNode.Children.Count == 0)
            {
                node.PostVisit(visitor);
                _stack.Pop();
                continue;
            }

            if (_visited.Contains(nonTerminatedNode))
            {
                node.PostVisit(visitor);
                _stack.Pop();
                continue;
            }

            _visited.Add(nonTerminatedNode);
            foreach (SyntaxNodeBase child in nonTerminatedNode.Children.AsEnumerable().Reverse())
            {
                _stack.Push(child);
            }
        }
    }
}
