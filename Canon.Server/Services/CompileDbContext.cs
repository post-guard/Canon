using Canon.Server.Models;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;

namespace Canon.Server.Services;

public class CompileDbContext : DbContext
{
    public DbSet<CompileResult> CompileResults { get; init; }

    public CompileDbContext(DbContextOptions<CompileDbContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<CompileResult>().ToCollection("compilerResults");
    }
}
