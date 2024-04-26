using Canon.Core.Abstractions;
using Canon.Core.CodeGenerators;
using Canon.Core.Enums;
using Canon.Core.LexicalParser;

namespace Canon.Core.SyntaxNodes;

public class OnAssignGeneratorEventArgs : EventArgs
{
    public required Variable Variable { get; init; }

    public required Expression Expression { get; init; }
}

public class OnReturnGeneratorEventArgs : EventArgs
{
    public required IdentifierSemanticToken FunctionName { get; set; }

    public required Expression Expression { get; init; }
}

public class OnIfGeneratorEventArgs : EventArgs
{
    public required Expression Condition { get; init; }

    public required Statement Sentence { get; init; }

    public required ElsePart ElseSentence { get; init; }
}

public class OnForGeneratorEventArgs : EventArgs
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
    public event EventHandler<OnAssignGeneratorEventArgs>? OnAssignGenerator;

    /// <summary>
    /// 使用返回产生式的事件
    /// </summary>
    public event EventHandler<OnReturnGeneratorEventArgs>? OnReturnGenerator;

    /// <summary>
    /// 使用If产生式的事件
    /// </summary>
    public event EventHandler<OnIfGeneratorEventArgs>? OnIfGenerator;

    /// <summary>
    /// 使用For产生式的事件
    /// </summary>
    public event EventHandler<OnForGeneratorEventArgs>? OnForGenerator;

    public static Statement Create(List<SyntaxNodeBase> children)
    {
        return new Statement { Children = children };
    }

    private void RaiseEvent()
    {
        if (Children.Count == 2)
        {
            if (Children[0].IsTerminated)
            {
                OnReturnGenerator?.Invoke(this, new OnReturnGeneratorEventArgs
                {
                    FunctionName = Children[0].Convert<TerminatedSyntaxNode>().Token.Convert<IdentifierSemanticToken>(),
                    Expression = Children[2].Convert<Expression>()
                });
            }
            else
            {
                OnAssignGenerator?.Invoke(this, new OnAssignGeneratorEventArgs
                {
                    Variable = Children[0].Convert<Variable>(),
                    Expression = Children[2].Convert<Expression>()
                });
            }
        }
        else if (Children.Count == 5)
        {
            OnIfGenerator?.Invoke(this, new OnIfGeneratorEventArgs
            {
                Condition = Children[1].Convert<Expression>(),
                Sentence = Children[3].Convert<Statement>(),
                ElseSentence = Children[4].Convert<ElsePart>()
            });
        }
        else if (Children.Count == 8)
        {
            OnForGenerator?.Invoke(this, new OnForGeneratorEventArgs
            {
                Iterator = Children[1].Convert<TerminatedSyntaxNode>().Token.Convert<IdentifierSemanticToken>(),
                Begin = Children[3].Convert<Expression>(),
                End = Children[5].Convert<Expression>(),
                Sentence = Children[7].Convert<Statement>()
            });
        }
    }

    public override void GenerateCCode(CCodeBuilder builder)
    {
        if (Children.Count == 0)
        {
            return;
        }

        // statement -> procedureCall | compoundStatement
        if (Children.Count == 1)
        {
            Children[0].GenerateCCode(builder);
        }
        //statement -> variable assign expression
        else if (Children.Count == 3)
        {
            Children[0].GenerateCCode(builder);
            builder.AddString(" =");
            Children[2].GenerateCCode(builder);
        }
        //if expression then statement else_part
        else if (Children.Count == 5)
        {
            builder.AddString(" if(");
            Children[1].GenerateCCode(builder);
            builder.AddString("){");
            Children[3].GenerateCCode(builder);
            builder.AddString("; }");
            Children[4].GenerateCCode(builder);
        }
        //for id assign expression to expression do statement
        else
        {
            string idName = Children[1].Convert<TerminatedSyntaxNode>().Token.Convert<IdentifierSemanticToken>()
                .IdentifierName;
            builder.AddString(" for(" + idName + " =");
            Children[3].GenerateCCode(builder);
            builder.AddString("; " + idName + " <=");
            Children[5].GenerateCCode(builder);
            builder.AddString("; " + idName + "++){");
            Children[7].GenerateCCode(builder);
            builder.AddString("; }");
        }
    }
}
