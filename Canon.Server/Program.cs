using Canon.Core.Abstractions;
using Canon.Core.GrammarParser;
using Canon.Core.LexicalParser;
using Canon.Core.SemanticParser;
using Canon.Server.Extensions;
using Canon.Server.Models;
using Canon.Server.Services;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string? connectionString = builder.Configuration.GetConnectionString("MongoDB");
if (connectionString is null)
{
    throw new InvalidOperationException("Failed to get MongoDB connection string.");
}

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<CompileDbContext>(options =>
{
    options.UseMongoDB(connectionString, "Canon");
});
builder.Services.AddGridFs(connectionString, "Canon");
builder.Services.AddSingleton<ICompilerLogger, CompilerLogger>();
builder.Services.AddTransient<ILexer, Lexer>();
builder.Services.AddSingleton<IGrammarParser>(
    _ => GeneratedGrammarParser.Instance);
builder.Services.AddSingleton<SyntaxTreePresentationService>();
builder.Services.AddSingleton<SyntaxTreeTraveller>();
builder.Services.AddTransient<CompilerService>();
builder.Services.AddHostedService<DatabaseSetupService>();

WebApplication application = builder.Build();

if (application.Environment.IsDevelopment())
{
    application.UseSwagger();
    application.UseSwaggerUI();
}

application.UseStaticFiles();

application.MapControllers();
application.MapFallbackToFile("/index.html");

await application.RunAsync();
