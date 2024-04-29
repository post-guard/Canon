using Canon.Server.Entities;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;

namespace Canon.Server.Services;

public class CompileDbContext(DbContextOptions<CompileDbContext> options) : DbContext(options)
{
    public DbSet<CompileResult> CompileResults { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<CompileResult>().ToCollection("compilerResults");
    }
}
