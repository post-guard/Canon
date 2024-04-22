namespace Canon.Server.Services;

public class DatabaseSetupService(IServiceProvider serviceProvider, ILogger<DatabaseSetupService> logger)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        CompileDbContext dbContext = scope.ServiceProvider.GetRequiredService<CompileDbContext>();

        logger.LogInformation("Clear old entities in database.");
        dbContext.CompileResults.RemoveRange(dbContext.CompileResults);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
