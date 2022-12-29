using Modules.Storage.Core.Abstractions;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace Modules.Storage.Infrastructure.Persistence;

public class GridFsFileRepository<TMetadata> : IGridFsRepository<TMetadata>
{
    private readonly IGridFSBucket _gridFsBucket;

    public GridFsFileRepository(MongoContext mongoContext)
    {
        _gridFsBucket = new GridFSBucket(mongoContext.MongoDatabase);
    }

    public async Task<string> UploadFileAsync(string fileName, TMetadata metadata, Stream stream)
    {
        var objectId = await _gridFsBucket.UploadFromStreamAsync(fileName, stream, new GridFSUploadOptions
        {
            Metadata = metadata.ToBsonDocument()
        });

        return objectId.ToString();
    }

    public async Task<List<GridFSFileInfo>> ListFileMetadataAsync(FilterDefinition<GridFSFileInfo> filter)
    {
        using var asyncCursor = await _gridFsBucket.FindAsync(filter);
        return await asyncCursor.ToListAsync();
    }

    public async Task DeleteManyAsync(FilterDefinition<GridFSFileInfo> filter)
    {
        var taskList = new List<Task>();
        var fileList = await ListFileMetadataAsync(filter);
        foreach (var eachFile in fileList)
        {
            taskList.Add(_gridFsBucket.DeleteAsync(eachFile.Id));
            if (taskList.Count >= 10)
            {
                await Task.WhenAll(taskList);
                taskList.Clear();
            }
        }

        await Task.WhenAll(taskList);
    }
}