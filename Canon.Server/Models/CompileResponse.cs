using System.ComponentModel.DataAnnotations;

namespace Canon.Server.Models;

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

    public CompileResponse()
    {
        Id = string.Empty;
        SourceCode = string.Empty;
        CompiledCode = string.Empty;
        ImageAddress = string.Empty;
    }

    public CompileResponse(CompileResult result)
    {
        Id = result.CompileId;
        SourceCode = result.SourceCode;
        CompiledCode = result.CompiledCode;
        ImageAddress = $"/api/file/{result.SytaxTreeImageFilename}";
    }
}
