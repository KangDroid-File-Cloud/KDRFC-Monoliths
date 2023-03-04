using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace Modules.Storage.Core.Abstractions;

public interface IGridFsRepository<TMetadata>
{
    Task<string> UploadFileAsync(string fileName, TMetadata metadata, Stream stream);
    public Task<List<GridFSFileInfo>> ListFileMetadataAsync(FilterDefinition<GridFSFileInfo> filter);

    public Task<GridFSFileInfo?> GetFileById(string id);

    public Task DeleteManyAsync(FilterDefinition<GridFSFileInfo> filter);

    public Task<GridFSDownloadStream<ObjectId>> OpenDownloadStreamAsync(
        string id, CancellationToken cancellationToken = default);
}