using Canon.Core.Abstractions;
using Canon.Core.CodeGenerators;
using Canon.Core.Enums;

namespace Canon.Core.SyntaxNodes;

public class StatementList : NonTerminatedSyntaxNode
{
    public override NonTerminatorType Type => NonTerminatorType.StatementList;

    public bool IsRecursive { get; private init; }

    public IEnumerable<Statement> Statements => GetStatements();

    public override void PreVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PreVisit(this);
    }

    public override void PostVisit(SyntaxNodeVisitor visitor)
    {
        visitor.PostVisit(this);
    }

    public static StatementList Create(List<SyntaxNodeBase> children)
    {
        bool isRecursive;

        if (children.Count == 1)
        {
            isRecursive = false;
        }
        else if (children.Count == 3)
        {
            isRecursive = true;
        }
        else
        {
            throw new InvalidOperationException();
        }

        return new StatementList { Children = children, IsRecursive = isRecursive };
    }

    private IEnumerable<Statement> GetStatements()
    {
        StatementList list = this;

        while (true)
        {
            if (list.IsRecursive)
            {
                yield return list.Children[2].Convert<Statement>();
                list = list.Children[0].Convert<StatementList>();
            }
            else
            {
                yield return list.Children[0].Convert<Statement>();
                break;
            }
        }
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
