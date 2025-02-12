﻿using System.Collections;
using Canon.Core.Enums;
using Canon.Core.SemanticParser;

namespace Canon.Core.SyntaxNodes;

public abstract class NonTerminatedSyntaxNode : SyntaxNodeBase, IEnumerable<SyntaxNodeBase>
{
    public override bool IsTerminated => false;

    public abstract NonTerminatorType Type { get; }

    public required List<SyntaxNodeBase> Children { get; init; }

    private PascalType? _variableType;

    private string? _variableName;

    public PascalType VariableType
    {
        get
        {
            if (_variableType is null)
            {
                throw new InvalidOperationException("Did not set the type of the node");
            }

            return _variableType;
        }
        set
        {
            _variableType = value;
        }
    }

    public string VariableName
    {
        get
        {
            if (_variableName is null)
            {
                throw new InvalidOperationException("Did not set the name of the node");
            }

            return _variableName;
        }
        set
        {
            _variableName = value;
        }
    }

    public IEnumerator<SyntaxNodeBase> GetEnumerator()
    {
        yield return this;

        foreach (SyntaxNodeBase child in Children)
        {
            if (child.IsTerminated)
            {
                yield return child;
            }
            else
            {
                NonTerminatedSyntaxNode nonTerminatedNode = child.Convert<NonTerminatedSyntaxNode>();

                foreach (SyntaxNodeBase node in nonTerminatedNode)
                {
                    yield return node;
                }
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString()
    {
        return Type.ToString();
    }
}
