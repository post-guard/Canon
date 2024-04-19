using Canon.Server.Services;
using MongoDB.Driver;

namespace Canon.Server.Extensions;

public static class ServiceProviderExtensions
{
    public static void AddGridFs(this IServiceCollection serviceCollection,
        string connectionString, string databaseName)
    {
        serviceCollection.AddSingleton<IMongoClient, MongoClient>(
            _ => new MongoClient(connectionString));

        serviceCollection.AddScoped<GridFsService>(provider =>
        {
            IMongoClient client = provider.GetRequiredService<IMongoClient>();
            return new GridFsService(client, databaseName);
        });
    }
}
