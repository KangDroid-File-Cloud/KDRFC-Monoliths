using System.Net;
using MediatR;
using Modules.Storage.Core.Abstractions;
using Modules.Storage.Core.Extensions;
using Modules.Storage.Core.Models;
using Modules.Storage.Core.Models.Data;
using Modules.Storage.Core.Models.Responses;
using Shared.Core.Abstractions;
using Shared.Core.Exceptions;

namespace Modules.Storage.Core.Commands.Handlers;

public class CheckBlobEligibleCommandHandler : IRequestHandler<CheckBlobEligibleCommand, BlobEligibleResponse>
{
    private readonly ICacheService _cacheService;
    private readonly IGridFsRepository<BlobFile> _gridFsRepository;

    public CheckBlobEligibleCommandHandler(IGridFsRepository<BlobFile> gridFsRepository, ICacheService cacheService)
    {
        _gridFsRepository = gridFsRepository;
        _cacheService = cacheService;
    }

    public async Task<BlobEligibleResponse> Handle(CheckBlobEligibleCommand request, CancellationToken cancellationToken)
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

        var blobEligible = new BlobEligibleResponse
        {
            Token = Ulid.NewUlid().ToString()
        };

        await _cacheService.SetItemAsync(StorageCacheKeys.TempBlobDownloadKey(request.BlobId), blobEligible,
            TimeSpan.FromMinutes(1));

        return blobEligible;
    }
}