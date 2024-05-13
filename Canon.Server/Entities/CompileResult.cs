using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace Canon.Server.Entities;

public class CompileResult
{
    public ObjectId Id { get; set; }

    public bool Error { get; set; }

    public string SourceCode { get; set; } = string.Empty;

    [MaxLength(40)]
    public string SytaxTreeImageFilename { get; set; } = string.Empty;

    public string CompiledCode { get; set; } = string.Empty;

    public string CompileInformation { get; set; } = string.Empty;

    public DateTime CompileTime { get; set; }
}
