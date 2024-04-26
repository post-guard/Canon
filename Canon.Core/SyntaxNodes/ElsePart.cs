﻿using Canon.Core.Abstractions;
using Canon.Core.CodeGenerators;
using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class ElsePart : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.ElsePart;

    public override void PreVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PreVisit(this);
    }

    public override void PostVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PostVisit(this);
    }

    public static ElsePart Create(List<SyntaxNodeBase> children)
    {
        return new ElsePart { Children = children };
    }

    public override void GenerateCCode(CCodeBuilder builder)
    {
        if (Children.Count > 0)
        {
            builder.AddString(" else{");
            Children[1].GenerateCCode(builder);
            builder.AddString(" }");
        }
    }
}
