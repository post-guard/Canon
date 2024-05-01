using Canon.Core.Abstractions;
using Canon.Core.CodeGenerators;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Core.SyntaxNodes;

public class AssignGeneratorEventArgs : EventArgs
{
    public required Variable Variable { get; init; }

    public required Expression Expression { get; init; }
}

public class IfGeneratorEventArgs : EventArgs
{
    public required Expression Condition { get; init; }

    public required Statement Sentence { get; init; }

    public required ElsePart ElseSentence { get; init; }
}

public class ForGeneratorEventArgs : EventArgs
{
    public required IdentifierSemanticToken Iterator { get; init; }

    public required Expression Begin { get; init; }

    public required Expression End { get; init; }

    public required Statement Sentence { get; init; }
}

public class Statement : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.Statement;

    public override void PreVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PreVisit(this);
        RaiseEvent();
    }

    public override void PostVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PostVisit(this);
        RaiseEvent();
    }

    /// <summary>
    /// 使用赋值产生式的事件
    /// </summary>
    public event EventHandler<AssignGeneratorEventArgs>? OnAssignGenerator;

    /// <summary>
    /// 使用If产生式的事件
    /// </summary>
    public event EventHandler<IfGeneratorEventArgs>? OnIfGenerator;

    /// <summary>
    /// 使用For产生式的事件
    /// </summary>
    public event EventHandler<ForGeneratorEventArgs>? OnForGenerator;

    public static Statement Create(List<SyntaxNodeBase> children)
    {
        return new Statement { Children = children };
    }

    private void RaiseEvent()
    {
        if (Children.Count == 3)
        {
            OnAssignGenerator?.Invoke(this,
                new AssignGeneratorEventArgs
                {
                    Variable = Children[0].Convert<Variable>(), Expression = Children[2].Convert<Expression>()
                });
        }
        else if (Children.Count == 5)
        {
            OnIfGenerator?.Invoke(this,
                new IfGeneratorEventArgs
                {
                    Condition = Children[1].Convert<Expression>(),
                    Sentence = Children[3].Convert<Statement>(),
                    ElseSentence = Children[4].Convert<ElsePart>()
                });
        }
        else if (Children.Count == 8)
        {
            OnForGenerator?.Invoke(this,
                new ForGeneratorEventArgs
                {
                    Iterator = Children[1].Convert<TerminatedSyntaxNode>().Token.Convert<IdentifierSemanticToken>(),
                    Begin = Children[3].Convert<Expression>(),
                    End = Children[5].Convert<Expression>(),
                    Sentence = Children[7].Convert<Statement>()
                });
        }
    }
}
