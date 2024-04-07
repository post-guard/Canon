﻿using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class SubprogramDeclarations : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.SubprogramDeclarations;

    /// <summary>
    /// 声明的子程序列表
    /// </summary>
    public IEnumerable<Subprogram> Subprograms => GetSubprograms();

    public static SubprogramDeclarations Create(List<SyntaxNodeBase> children)
    {
        return new SubprogramDeclarations { Children = children };
    }

    private IEnumerable<Subprogram> GetSubprograms()
    {
        SubprogramDeclarations declarations = this;

        while (true)
        {
            if (declarations.Children.Count == 0)
            {
                yield break;
            }

            yield return declarations.Children[1].Convert<Subprogram>();
            declarations = declarations.Children[0].Convert<SubprogramDeclarations>();
        }
    }
}
