using Modules.Storage.Core.Models;
using Modules.Storage.Core.Models.Responses;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.GridFS;

namespace Modules.Storage.Core.Extensions;

public static class GridFSFileInfoExtension
{
    public static BlobProjection ToBlobProjection(this GridFSFileInfo fileInfo)
    {
        var blobMetadata = BsonSerializer.Deserialize<BlobFile>(fileInfo.Metadata);

        return new BlobProjection
        {
            Id = fileInfo.Id.ToString(),
            Name = fileInfo.Filename,
            Length = fileInfo.Length,
            UploadDate = fileInfo.UploadDateTime,
            BlobFileType = blobMetadata.BlobFileType,
            ParentFolderId = blobMetadata.ParentFolderId
        };
    }

    public static BlobFile ToBlobFile(this GridFSFileInfo fileInfo)
    {
        return BsonSerializer.Deserialize<BlobFile>(fileInfo.Metadata);
    }
}