using Canon.Core.CodeGenerators;
using Canon.Core.SyntaxNodes;

namespace Canon.Core.SemanticParser;

public class CCodeGenerateVisitor : TypeCheckVisitor
{
    public CCodeBuilder Builder { get; } = new();

    public override void PreVisit(ProgramStruct programStruct)
    {
        base.PreVisit(programStruct);

        Builder.AddString("#include <stdbool.h>\n");
        Builder.AddString("#include <stdio.h>\n");
    }
}
