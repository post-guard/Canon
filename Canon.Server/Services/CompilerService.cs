using Canon.Core.Abstractions;
using Canon.Core.LexicalParser;
using Canon.Core.SemanticParser;
using Canon.Core.SyntaxNodes;
using Canon.Server.DataTransferObjects;
using Canon.Server.Entities;
using Canon.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace Canon.Server.Services;

public class CompilerService(
    ILexer lexer,
    IGrammarParser grammarParser,
    SyntaxTreeTraveller traveller,
    ICompilerLogger compilerLogger,
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

        CodeGeneratorVisitor visitor = new();
        traveller.Travel(root, visitor);

        CompileResult result = new()
        {
            SourceCode = sourceCode.Code,
            CompileId = Guid.NewGuid().ToString(),
            CompiledCode = visitor.Builder.Build(),
            SytaxTreeImageFilename = filename,
            CompileTime = DateTime.Now,
            CompileInformation = compilerLogger.Build()
        };

        await dbContext.CompileResults.AddAsync(result);
        await dbContext.SaveChangesAsync();

        return new CompileResponse(result);
    }
}
