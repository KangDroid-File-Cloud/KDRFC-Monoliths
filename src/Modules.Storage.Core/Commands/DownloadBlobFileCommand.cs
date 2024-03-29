using MediatR;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;

namespace Modules.Storage.Core.Commands;

public class DownloadBlobFileCommand : IRequest<GridFSDownloadStream<ObjectId>>
{
    /// <summary>
    ///     Blob ID to download
    /// </summary>
    public string BlobId { get; set; }

    /// <summary>
    ///     Blob Access Token
    /// </summary>
    public string BlobAccessToken { get; set; }
}