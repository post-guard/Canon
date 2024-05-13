using Canon.Server.DataTransferObjects;
using Canon.Server.Entities;
using Canon.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;

namespace Canon.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompilerController(CompileDbContext dbContext, CompilerService compilerService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CompileResponse>>> ListResponses([FromQuery] int start = 1, [FromQuery] int end = 20)
    {
        if (end <= start || start < 1)
        {
            return BadRequest();
        }

        IQueryable<CompileResult> results = from item in dbContext.CompileResults.AsNoTracking()
            orderby item.CompileTime descending
            select item;

        IEnumerable<CompileResult> cachedResults = await results.ToListAsync();

        IEnumerable<CompileResponse> responses = cachedResults.Skip(start - 1)
            .Take(end - start + 1)
            .Select(result => new CompileResponse(result));

        return Ok(responses);
    }

    [HttpGet("{compileId}")]
    public async Task<ActionResult<CompileResponse>> GetResponse(string compileId)
    {
        CompileResult? result = await (from item in dbContext.CompileResults.AsNoTracking()
            where item.Id == new ObjectId(compileId)
            select item).FirstOrDefaultAsync();

        if (result is null)
        {
            return NotFound();
        }

        return Ok(new CompileResponse(result));
    }

    [HttpPost]
    public async Task<ActionResult<CompileResponse>> Compile(SourceCode sourceCode)
    {
        CompileResponse response = await compilerService.Compile(sourceCode);
        return Ok(response);
    }

    [HttpDelete("{compileId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteCompileResult(string compileId)
    {
        CompileResult? result = await (from item in dbContext.CompileResults
            where item.Id == new ObjectId(compileId)
            select item).FirstOrDefaultAsync();

        if (result is null)
        {
            return NotFound();
        }

        dbContext.CompileResults.Remove(result);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteAllCompileResult()
    {
        dbContext.CompileResults.RemoveRange(dbContext.CompileResults);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }
}
