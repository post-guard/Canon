﻿using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class Subprogram : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.Subprogram;

    /// <summary>
    /// 子程序头部
    /// </summary>
    public SubprogramHead Head => Children[0].Convert<SubprogramHead>();

    /// <summary>
    /// 子程序体
    /// </summary>
    public SubprogramBody Body => Children[2].Convert<SubprogramBody>();

    public static Subprogram Create(List<SyntaxNodeBase> children)
    {
        return new Subprogram { Children = children };
    }
}
