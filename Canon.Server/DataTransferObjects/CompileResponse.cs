using System.ComponentModel.DataAnnotations;
using Canon.Server.Entities;

namespace Canon.Server.DataTransferObjects;

public class CompileResponse
{
    [Required]
    public string Id { get; set; }

    [Required]
    public string SourceCode { get; set; }

    [Required]
    public string CompiledCode { get; set; }

    [Required]
    public string ImageAddress { get; set; }

    [Required]
    public string CompileTime { get; set; }

    public CompileResponse()
    {
        Id = string.Empty;
        SourceCode = string.Empty;
        CompiledCode = string.Empty;
        ImageAddress = string.Empty;
        CompileTime = string.Empty;
    }

    public CompileResponse(CompileResult result)
    {
        Id = result.CompileId;
        SourceCode = result.SourceCode;
        CompiledCode = result.CompiledCode;
        ImageAddress = $"/api/file/{result.SytaxTreeImageFilename}";
        CompileTime = result.CompileTime.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss");
    }
}
