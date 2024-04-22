using Canon.Core.Abstractions;
using Canon.Core.CodeGenerators;
using Canon.Core.GrammarParser;
using Canon.Core.LexicalParser;
using Canon.Core.SyntaxNodes;
using Canon.Tests.Utils;
using Xunit.Abstractions;

namespace Canon.Tests.CCodeGeneratorTests;

public class StatementTests
{
    private readonly IGrammarParser _parser = GeneratedGrammarParser.Instance;
    private readonly ILexer _lexer = new Lexer();
    private readonly ITestOutputHelper _outputHelper;

    public StatementTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public void VariableAssignTest()
    {
        CCodeBuilder builder = new();

        const string program = """
                               program varAssignTest;
                               var a, b: integer;
                               begin
                               a := 1;
                               b := a;
                               a := b;
                               end.
                               """;

        IEnumerable<SemanticToken> tokens = _lexer.Tokenize(new StringSourceReader(program));

        ProgramStruct root = _parser.Analyse(tokens);
        root.GenerateCCode(builder);

        string result = builder.Build();
        _outputHelper.WriteLine(result);
        Assert.Equal("#include <PascalCoreLib.h> #include <stdbool.h> " +
                     "int a, b; int main(){ a = 1; b = a; a = b; return 0;}", result);
    }

    [Fact]
    public void IfTest()
    {
        CCodeBuilder builder = new();

        const string program = """
                               program main;
                               var
                               a,b:integer;
                               begin
                               if a = 5 then
                                   begin
                                   if b = 3 then
                                       b := b + 1
                                   else
                                       b := b + 2
                                   end
                               else
                                       a := 2
                               end.

                               """;

        IEnumerable<SemanticToken> tokens = _lexer.Tokenize(new StringSourceReader(program));

        ProgramStruct root = _parser.Analyse(tokens);
        root.GenerateCCode(builder);

        string result = builder.Build();
        _outputHelper.WriteLine(result);
        Assert.Equal("#include <PascalCoreLib.h> #include <stdbool.h> int a, b; " +
                     "int main(){ a = 1; if( a == 1){ a = a + 1; } else{ b = a + 2 }; return 0;}", result);
    }

    [Fact]
    public void ForLoopTest()
    {
        CCodeBuilder builder = new();

        const string program = """
                               program ForLoopTest;
                               var a, b, c: integer;
                               begin
                               b := 1;
                               for a := b * 5 + 1 to 99 do
                               begin
                                    c := a + 1;
                                    b := a mod (a + c);
                                    b := a + 1 - 1 * 1;
                               end;
                               end.
                               """;

        IEnumerable<SemanticToken> tokens = _lexer.Tokenize(new StringSourceReader(program));

        ProgramStruct root = _parser.Analyse(tokens);
        root.GenerateCCode(builder);

        string result = builder.Build();
        _outputHelper.WriteLine(result);
        Assert.Equal("#include <PascalCoreLib.h> #include <stdbool.h> int a, b; " +
                     "int main(){ a = 1; if( a == 1){ a = a + 1; } else{ b = a + 2 }; return 0;}", result);
    }
}
