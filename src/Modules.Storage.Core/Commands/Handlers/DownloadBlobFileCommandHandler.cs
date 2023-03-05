using System.Net;
using MediatR;
using Modules.Storage.Core.Abstractions;
using Modules.Storage.Core.Models;
using Modules.Storage.Core.Models.Data;
using Modules.Storage.Core.Models.Responses;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using Shared.Core.Abstractions;
using Shared.Core.Exceptions;

namespace Modules.Storage.Core.Commands.Handlers;

public class DownloadBlobFileCommandHandler : IRequestHandler<DownloadBlobFileCommand, GridFSDownloadStream<ObjectId>>
{
    private readonly ICacheService _cacheService;
    private readonly IGridFsRepository<BlobFile> _gridFsRepository;

    public DownloadBlobFileCommandHandler(IGridFsRepository<BlobFile> gridFsRepository, ICacheService cacheService)
    {
        _gridFsRepository = gridFsRepository;
        _cacheService = cacheService;
    }

    public async Task<GridFSDownloadStream<ObjectId>> Handle(DownloadBlobFileCommand request,
                                                             CancellationToken cancellationToken)
    {
        var targetKey =
            await _cacheService.GetItemAsync<BlobEligibleResponse>(StorageCacheKeys.TempBlobDownloadKey(request.BlobId))
            ?? throw new ApiException(HttpStatusCode.Unauthorized, $"Cannot access blob {request.BlobId}");

        // Check whether Blob Access Token matches
        if (targetKey.Token != request.BlobAccessToken)
        {
            throw new ApiException(HttpStatusCode.Unauthorized, $"Cannot access blob {request.BlobId}");
        }

        return await _gridFsRepository.OpenDownloadStreamAsync(request.BlobId, cancellationToken);
    }
}