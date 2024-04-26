using System.Text;

namespace Canon.Generator.SyntaxVisitorGenerator;

public static class SyntaxVisitorGenerator
{
    private static readonly List<string> s_nodes =
    [
        "AddOperator",
        "BasicType",
        "CompoundStatement",
        "ConstDeclaration",
        "ConstDeclarations",
        "ConstValue",
        "ElsePart",
        "Expression",
        "ExpressionList",
        "Factor",
        "FormalParameter",
        "IdentifierList",
        "IdentifierVarPart",
        "MultiplyOperator",
        "Parameter",
        "ParameterList",
        "Period",
        "ProcedureCall",
        "ProgramBody",
        "ProgramHead",
        "ProgramStruct",
        "RelationOperator",
        "SimpleExpression",
        "Statement",
        "StatementList",
        "Subprogram",
        "SubprogramBody",
        "SubprogramDeclarations",
        "SubprogramHead",
        "Term",
        "TypeSyntaxNode",
        "ValueParameter",
        "VarDeclaration",
        "VarDeclarations",
        "Variable",
        "VarParameter"
    ];

    public static async Task Generate()
    {
        FileInfo output = new(Path.Combine(Environment.CurrentDirectory, "SyntaxNodeVisitor.cs"));

        StringBuilder builder = new();

        builder.Append("using Canon.Core.SyntaxNodes;\n").Append('\n');
        builder.Append("namespace Canon.Core.Abstractions;\n").Append('\n');

        builder.Append("public abstract class SyntaxNodeVisitor\n").Append("{\n");

        foreach (string node in s_nodes)
        {
            string nodeName = node.Substring(0, 1).ToLower() + node.Substring(1);

            builder.Append($"    public virtual void PreVisit({node} {nodeName})\n")
                .Append("    {\n")
                .Append("    }\n")
                .Append('\n');

            builder.Append($"    public virtual void PostVisit({node} {nodeName})\n")
                .Append("    {\n")
                .Append("    }\n")
                .Append('\n');
        }

        builder.Append('}');

        await using StreamWriter writer = output.CreateText();
        await writer.WriteAsync(builder.ToString());
    }
}
