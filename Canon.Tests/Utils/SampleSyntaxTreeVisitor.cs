using System.Text;
using Canon.Core.Abstractions;
using Canon.Core.SyntaxNodes;

namespace Canon.Tests.Utils;

public class SampleSyntaxTreeVisitor : SyntaxNodeVisitor
{
    private readonly StringBuilder _builder = new();

    public override string ToString()
    {
        return _builder.ToString();
    }

    public override void PreVisit(ProgramStruct programStruct)
    {
        _builder.Append(programStruct).Append('\n');
    }

    public override void PostVisit(ProgramStruct programStruct)
    {
        _builder.Append(programStruct).Append('\n');
    }

    public override void PreVisit(ProgramHead programHead)
    {
        _builder.Append(programHead).Append('\n');
    }

    public override void PostVisit(ProgramHead programHead)
    {
        _builder.Append(programHead).Append('\n');
    }

    public override void PreVisit(ProgramBody programBody)
    {
        _builder.Append(programBody).Append('\n');
    }

    public override void PostVisit(ProgramBody programBody)
    {
        _builder.Append(programBody).Append('\n');
    }

    public override void PreVisit(SubprogramDeclarations subprogramDeclarations)
    {
        _builder.Append(subprogramDeclarations).Append('\n');
    }

    public override void PostVisit(SubprogramDeclarations subprogramDeclarations)
    {
        _builder.Append(subprogramDeclarations).Append('\n');
    }

    public override void PreVisit(CompoundStatement compoundStatement)
    {
        _builder.Append(compoundStatement).Append('\n');
    }

    public override void PostVisit(CompoundStatement compoundStatement)
    {
        _builder.Append(compoundStatement).Append('\n');
    }

    public override void PreVisit(StatementList statementList)
    {
        _builder.Append(statementList).Append('\n');
    }

    public override void PostVisit(StatementList statementList)
    {
        _builder.Append(statementList).Append('\n');
    }

    public override void PreVisit(Statement statement)
    {
        _builder.Append(statement).Append('\n');
    }

    public override void PostVisit(Statement statement)
    {
        _builder.Append(statement).Append('\n');
    }
}
