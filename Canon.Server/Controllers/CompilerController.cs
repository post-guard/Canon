using Canon.Server.Models;
using Canon.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Canon.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompilerController(CompileDbContext dbContext, CompilerService compilerService) : ControllerBase
{
    [HttpGet("{compileId}")]
    public async Task<ActionResult<CompileResponse>> GetResponse(string compileId)
    {
        CompileResult? result = await dbContext.CompileResults
            .Where(r => r.CompileId == compileId)
            .FirstOrDefaultAsync();

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CompileResponse>> Compile(SourceCode sourceCode)
    {
        CompileResponse response = await compilerService.Compile(sourceCode);
        return Ok(response);
    }
}
