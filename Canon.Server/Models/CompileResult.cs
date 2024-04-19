using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace Canon.Server.Models;

public class CompileResult
{
    public ObjectId Id { get; set; }

    [MaxLength(40)]
    public string CompileId { get; set; } = string.Empty;

    public string SourceCode { get; set; } = string.Empty;

    [MaxLength(40)]
    public string SytaxTreeImageFilename { get; set; } = string.Empty;

    public string CompiledCode { get; set; } = string.Empty;
}
