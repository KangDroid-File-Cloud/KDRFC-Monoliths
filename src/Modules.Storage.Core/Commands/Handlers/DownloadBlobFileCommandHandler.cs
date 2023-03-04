using System.Net;
using MediatR;
using Modules.Storage.Core.Abstractions;
using Modules.Storage.Core.Extensions;
using Modules.Storage.Core.Models;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using Shared.Core.Exceptions;

namespace Modules.Storage.Core.Commands.Handlers;

public class DownloadBlobFileCommandHandler : IRequestHandler<DownloadBlobFileCommand, GridFSDownloadStream<ObjectId>>
{
    private readonly IGridFsRepository<BlobFile> _gridFsRepository;

    public DownloadBlobFileCommandHandler(IGridFsRepository<BlobFile> gridFsRepository)
    {
        _gridFsRepository = gridFsRepository;
    }

    public async Task<GridFSDownloadStream<ObjectId>> Handle(DownloadBlobFileCommand request,
                                                             CancellationToken cancellationToken)
    {
        // Check whether blobFile exists.
        var blobFile = await _gridFsRepository.GetFileById(request.BlobId) ??
                       throw new ApiException(HttpStatusCode.NotFound, $"Cannot find blob: {request.BlobId}");

        // Check whether blob is user's one.
        if (blobFile.ToBlobFile().OwnerId != request.UserId)
        {
            throw new ApiException(HttpStatusCode.Forbidden, $"Blob {request.BlobId} is not user's one!");
        }

        // Check whether blob is not a file.
        if (blobFile.ToBlobFile().BlobFileType == BlobFileType.Folder)
        {
            throw new ApiException(HttpStatusCode.BadRequest, $"Blob {request.BlobId} is not a file!");
        }

        return await _gridFsRepository.OpenDownloadStreamAsync(request.BlobId, cancellationToken);
    }
}