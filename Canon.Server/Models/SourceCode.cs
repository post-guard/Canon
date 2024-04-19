using System.ComponentModel.DataAnnotations;

namespace Canon.Server.Models;

public class SourceCode
{
    [Required]
    public required string Code { get; set; }
}
