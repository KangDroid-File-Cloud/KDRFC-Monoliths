using System.Net;
using MediatR;
using Modules.Storage.Core.Abstractions;
using Modules.Storage.Core.Extensions;
using Modules.Storage.Core.Models;
using Modules.Storage.Core.Models.Responses;
using Shared.Core.Exceptions;

namespace Modules.Storage.Core.Commands.Handlers;

public class GetBlobDetailCommandHandler : IRequestHandler<GetBlobDetailCommand, BlobProjection>
{
    private readonly IGridFsRepository<BlobFile> _gridFsRepository;

    public GetBlobDetailCommandHandler(IGridFsRepository<BlobFile> gridFsRepository)
    {
        _gridFsRepository = gridFsRepository;
    }

    public async Task<BlobProjection> Handle(GetBlobDetailCommand request, CancellationToken cancellationToken)
    {
        var gridFsFileInfo = await _gridFsRepository.GetFileById(request.BlobId)
                             ?? throw new ApiException(HttpStatusCode.NotFound, $"Cannot find blob id with {request.BlobId}");
        if (gridFsFileInfo.ToBlobFile().OwnerId != request.AccountId)
        {
            throw new ApiException(HttpStatusCode.Forbidden,
                $"Cannot get blob information {request.BlobId} because blob is not owned by {request.AccountId}.");
        }

        return gridFsFileInfo.ToBlobProjection();
    }
}