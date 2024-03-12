using System.Collections;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Core.GrammarParser;

/// <summary>
/// 抽象语法树上的节点
/// </summary>
public class SyntaxNode : IEquatable<SyntaxNode>, IEnumerable<SyntaxNode>
{
    private readonly SemanticToken? _semanticToken;
    private readonly NonTerminatorType _nonTerminatorType;

    public bool IsTerminated { get; }

    public List<SyntaxNode> Children { get; } = [];

    public SyntaxNode(SemanticToken token)
    {
        IsTerminated = true;
        _semanticToken = token;
    }

    public SyntaxNode(NonTerminatorType nonTerminatorType)
    {
        IsTerminated = false;
        _nonTerminatorType = nonTerminatorType;
    }

    /// <summary>
    /// 获得终结节点包含的记号对象
    /// </summary>
    /// <returns>词法分析得到的记号对象</returns>
    /// <exception cref="InvalidOperationException">在非终结节点上调用该方法</exception>
    public SemanticToken GetSemanticToken()
    {
        if (!IsTerminated)
        {
            throw new InvalidOperationException("Can not get semantic token from a not terminated node");
        }

        return _semanticToken!;
    }

    /// <summary>
    /// 获得非终结节点的类型
    /// </summary>
    /// <returns>非终结节点类型</returns>
    /// <exception cref="InvalidOperationException">在终结节点上调用该方法</exception>
    public NonTerminatorType GetNonTerminatorType()
    {
        if (IsTerminated)
        {
            throw new InvalidOperationException("Can not get non terminated type from a terminated node");
        }

        return _nonTerminatorType;
    }

    public IEnumerator<SyntaxNode> GetEnumerator()
    {
        yield return this;

        foreach (SyntaxNode child in Children)
        {
            foreach (SyntaxNode node in child)
            {
                yield return node;
            }
        }
    }

    public bool Equals(SyntaxNode? other)
    {
        if (other is null)
        {
            return false;
        }

        if (IsTerminated != other.IsTerminated)
        {
            return false;
        }

        if (IsTerminated)
        {
            return GetSemanticToken() == other.GetSemanticToken();
        }
        else
        {
            // TODO: 在判等时是否需要判断子节点也相等
            return GetNonTerminatorType() == other.GetNonTerminatorType();
        }
    }

    public override bool Equals(object? obj)
    {
        if (obj is not SyntaxNode other)
        {
            return false;
        }

        return Equals(other);
    }

    public override int GetHashCode()
    {
        if (IsTerminated)
        {
            return GetSemanticToken().GetHashCode();
        }
        else
        {
            return GetNonTerminatorType().GetHashCode();
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static bool operator ==(SyntaxNode a, SyntaxNode b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(SyntaxNode a, SyntaxNode b)
    {
        return !a.Equals(b);
    }
}
