using Canon.Server.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver.GridFS;

namespace Canon.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileController(GridFsService gridFsService) : ControllerBase
{
    [HttpGet("{filename}")]
    public async Task<ActionResult> DownloadFile(string filename)
    {
        GridFSFileInfo? fileInfo = await gridFsService.FindAsync(filename);

        if (fileInfo is null)
        {
            return NotFound();
        }

        Stream fileStream = await gridFsService.OpenReadStream(fileInfo.Id);
        return File(fileStream, fileInfo.Metadata["content-type"].AsString);
    }
}
