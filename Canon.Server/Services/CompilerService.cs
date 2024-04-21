using Canon.Core.Abstractions;
using Canon.Core.LexicalParser;
using Canon.Core.SyntaxNodes;
using Canon.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace Canon.Server.Services;

public class CompilerService(
    ILexer lexer,
    IGrammarParser grammarParser,
    CompileDbContext dbContext,
    GridFsService gridFsService,
    SyntaxTreePresentationService syntaxTreePresentationService,
    ILogger<CompilerService> logger)
{
    public async Task<CompileResponse> Compile(SourceCode sourceCode)
    {
        logger.LogInformation("Try to compile: '{}'.", sourceCode.Code);

        IQueryable<CompileResult> resultQuery = from item in dbContext.CompileResults
            where item.SourceCode == sourceCode.Code
            select item;

        CompileResult? cachedResult = await resultQuery.FirstOrDefaultAsync();
        if (cachedResult is not null)
        {
            return new CompileResponse(cachedResult);
        }

        CodeReader reader = new(sourceCode);
        IEnumerable<SemanticToken> tokens = lexer.Tokenize(reader);
        ProgramStruct root = grammarParser.Analyse(tokens);

        await using Stream imageStream = syntaxTreePresentationService.Present(root);
        string filename = await gridFsService.UploadStream(imageStream, "image/png");

        CompileResult result = new()
        {
            SourceCode = sourceCode.Code,
            CompileId = Guid.NewGuid().ToString(),
            CompiledCode = string.Empty,
            SytaxTreeImageFilename = filename
        };

        await dbContext.CompileResults.AddAsync(result);
        await dbContext.SaveChangesAsync();

        return new CompileResponse(result);
    }
}
