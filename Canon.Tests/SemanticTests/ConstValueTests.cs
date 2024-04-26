using Canon.Core.Abstractions;
using Canon.Core.SemanticParser;
using Canon.Core.SyntaxNodes;
using Canon.Tests.Utils;

namespace Canon.Tests.SemanticTests;

public class ConstValueTests
{
    private class ConstValueVisitor : SyntaxNodeVisitor
    {
        public bool Pre { get; private set; }

        public bool Post { get; private set; }

        public override void PreVisit(ConstValue constValue)
        {
            constValue.OnNumberGenerator += (_, _) =>
            {
                Assert.False(Pre);
                Pre = true;
            };
        }

        public override void PostVisit(ConstValue constValue)
        {
            constValue.OnNumberGenerator += (_, _) =>
            {
                Assert.False(Post);
                Post = true;
            };
        }
    }

    [Fact]
    public void RaiseEventTest()
    {
        const string program = """
                               program main;
                               const a = 1;
                               begin
                               end.
                               """;

        ProgramStruct root = CompilerHelpers.Analyse(program);
        SyntaxTreeTraveller traveller = new();
        ConstValueVisitor visitor = new();
        traveller.Travel(root, visitor);

        Assert.True(visitor.Pre);
        Assert.True(visitor.Post);
    }
}
