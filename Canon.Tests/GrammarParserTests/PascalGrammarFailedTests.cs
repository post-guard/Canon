using Canon.Core.Exceptions;
using Canon.Tests.Utils;
using Xunit.Abstractions;

namespace Canon.Tests.GrammarParserTests;

public class PascalGrammarFailedTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void StructTest()
    {
        const string program = """
                               program main;
                               begin
                               end
                               """;

        CatchException(program);
    }

    [Fact]
    public void AssignTest()
    {
        const string program = """
                               program main;
                               begin
                                a := 'a';
                               end.
                               """;

        CatchException(program);
    }

    [Fact]
    public void StatementTest()
    {
        const string program = """
                               program main;
                               begin
                               if a = 1 then
                               doSomething;
                               else
                               doSomething;
                               end.
                               """;

        CatchException(program);
    }

    [Fact]
    public void ForTest()
    {
        const string program = """
                               program main;
                               begin
                               for a = 1 to 100 do
                                doSomething
                               end.
                               """;

        CatchException(program);
    }

    private void CatchException(string program)
    {
        GrammarException exception = Assert.Throws<GrammarException>(() =>
            CompilerHelpers.Analyse(program));

        testOutputHelper.WriteLine(exception.Message);
    }
}
