using System.ComponentModel.DataAnnotations;

namespace Canon.Server.DataTransferObjects;

public class SourceCode
{
    [Required]
    public required string Code { get; set; }
}
