using System.Text.Json;
using MediatR;
using Modules.Storage.Core.Abstractions;
using Modules.Storage.Core.Extensions;
using Modules.Storage.Core.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Shared.Core.Notifications;

namespace Modules.Storage.Core.Notifications.Handlers;

public class OnRemoveBlobDeleteHandler : INotificationHandler<OnRemoveBlobNotification>
{
    private readonly IGridFsRepository<BlobFile> _gridFsRepository;

    public OnRemoveBlobDeleteHandler(IGridFsRepository<BlobFile> gridFsRepository)
    {
        _gridFsRepository = gridFsRepository;
    }

    /// <summary>
    ///     Handle Blob Deletion Recursively.
    /// </summary>
    /// <remarks>This class is being tested with E2E(Integration) Test, Not Unit Tests.</remarks>
    /// <param name="notification"></param>
    /// <param name="cancellationToken"></param>
    public async Task Handle(OnRemoveBlobNotification notification, CancellationToken cancellationToken)
    {
        await HandleRecursive(notification.TargetBlobId, notification.AccountId);
    }

    private async Task HandleRecursive(string blobId, string accountId)
    {
        // List files under blobId
        var filter = Builders<GridFSFileInfo>.Filter.And(
            Builders<GridFSFileInfo>.Filter.Eq(
                a => a.Metadata[JsonNamingPolicy.CamelCase.ConvertName(nameof(BlobFile.OwnerId))], accountId),
            Builders<GridFSFileInfo>.Filter.Eq(
                a => a.Metadata[JsonNamingPolicy.CamelCase.ConvertName(nameof(BlobFile.ParentFolderId))], blobId));
        var fileList = (await _gridFsRepository.ListFileMetadataAsync(filter)).Select(a => a.ToBlobProjection());

        // foreach file list, remove it all
        foreach (var eachBlobFile in fileList)
        {
            // If blob type is folder, execute recursive again.
            if (eachBlobFile.BlobFileType == BlobFileType.Folder)
            {
                await HandleRecursive(eachBlobFile.Id, accountId);
            }
        }

        var deleteFilter = Builders<GridFSFileInfo>.Filter.Or(Builders<GridFSFileInfo>.Filter.Eq(
                a => a.Metadata[JsonNamingPolicy.CamelCase.ConvertName(nameof(BlobFile.ParentFolderId))], blobId),
            Builders<GridFSFileInfo>.Filter.Eq("_id", new ObjectId(blobId)));
        await _gridFsRepository.DeleteManyAsync(deleteFilter);
    }
}