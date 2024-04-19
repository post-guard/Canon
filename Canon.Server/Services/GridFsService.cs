using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace Canon.Server.Services;

public class GridFsService
{
    private readonly IGridFSBucket _bucket;

    public GridFsService(IMongoClient client, string databaseName)
    {
        IMongoDatabase database = client.GetDatabase(databaseName);
        _bucket = new GridFSBucket(database);
    }

    public async Task<GridFSFileInfo?> FindAsync(string filename)
    {
        FilterDefinition<GridFSFileInfo> filters = Builders<GridFSFileInfo>.Filter.Eq(
            file => file.Filename, filename);

        using IAsyncCursor<GridFSFileInfo> cursor = await _bucket.FindAsync(filters);

        return await cursor.FirstOrDefaultAsync();
    }

    public async Task<Stream> OpenReadStream(ObjectId id)
    {
        return await _bucket.OpenDownloadStreamAsync(id);
    }

    public async Task<string> UploadStream(Stream sourceStream, string contentType)
    {
        string filename = Guid.NewGuid().ToString();

        await _bucket.UploadFromStreamAsync(filename, sourceStream,
            new GridFSUploadOptions { Metadata = new BsonDocument { { "content-type", contentType } } });

        return filename;
    }
}
