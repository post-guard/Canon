using Canon.Core.Abstractions;
using Canon.Core.Exceptions;
using Canon.Core.LexicalParser;
using Canon.Core.SemanticParser;
using Canon.Core.SyntaxNodes;
using Canon.Server.DataTransferObjects;
using Canon.Server.Entities;
using Canon.Server.Models;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;

namespace Canon.Server.Services;

public class CompilerService(
    ILexer lexer,
    IGrammarParser grammarParser,
    SyntaxTreeTraveller traveller,
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

        ProgramStruct root;
        try
        {
            IEnumerable<SemanticToken> tokens = lexer.Tokenize(reader);
            root = grammarParser.Analyse(tokens);
        }
        catch (CanonException e)
        {
            CompileResult errorResult = new()
            {
                Id = ObjectId.GenerateNewId(),
                Error = true,
                SourceCode = sourceCode.Code,
                CompiledCode = string.Empty,
                SytaxTreeImageFilename = string.Empty,
                CompileTime = DateTime.Now,
                CompileInformation = e.Message
            };

            await dbContext.CompileResults.AddAsync(errorResult);
            await dbContext.SaveChangesAsync();

            return new CompileResponse(errorResult);
        }

        await using Stream imageStream = syntaxTreePresentationService.Present(root);
        string filename = await gridFsService.UploadStream(imageStream, "image/png");

        ICompilerLogger compilerLogger = new CompilerLogger();
        CodeGeneratorVisitor visitor = new(compilerLogger);
        traveller.Travel(root, visitor);

        CompileResult result = new()
        {
            Id = ObjectId.GenerateNewId(),
            Error = visitor.IsError,
            SourceCode = sourceCode.Code,
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
