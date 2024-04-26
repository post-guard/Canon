﻿using Canon.Core.Abstractions;
using Canon.Core.CodeGenerators;
using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class ProgramStruct : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.ProgramStruct;

    /// <summary>
    /// 程序头
    /// </summary>
    public ProgramHead Head => Children[0].Convert<ProgramHead>();

    /// <summary>
    /// 程序体
    /// </summary>
    public ProgramBody Body => Children[2].Convert<ProgramBody>();

    public override void PreVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PreVisit(this);
    }

    public override void PostVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PostVisit(this);
    }

    public static ProgramStruct Create(List<SyntaxNodeBase> children)
    {
        return new ProgramStruct { Children = children };
    }

    public override void GenerateCCode(CCodeBuilder builder)
    {
        builder.AddString("#include <stdbool.h>");
        Body.GenerateCCode(builder);
    }
}
