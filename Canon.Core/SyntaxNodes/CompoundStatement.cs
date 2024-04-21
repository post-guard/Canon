﻿using Canon.Core.CodeGenerators;
using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class CompoundStatement : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.CompoundStatement;

    public IEnumerable<Statement> Statements => Children[1].Convert<StatementList>().Statements;

    public static CompoundStatement Create(List<SyntaxNodeBase> children)
    {
        return new CompoundStatement { Children = children };
    }

    public override void GenerateCCode(CCodeBuilder builder)
    {
        foreach (var statement in Statements.Reverse())
        {
            if (statement.Children.Count > 0)
            {
                statement.GenerateCCode(builder);
                builder.AddString(";");
            }
        }
    }
}
