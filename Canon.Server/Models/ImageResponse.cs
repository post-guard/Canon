using System.ComponentModel.DataAnnotations;

namespace Canon.Server.Models;

public class ImageResponse
{
    [Required]
    public required string ResultId { get; set; }


}
