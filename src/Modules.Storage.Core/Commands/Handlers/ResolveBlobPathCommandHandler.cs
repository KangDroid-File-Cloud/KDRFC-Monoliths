using System.Net;
using MediatR;
using Modules.Storage.Core.Abstractions;
using Modules.Storage.Core.Extensions;
using Modules.Storage.Core.Models;
using Modules.Storage.Core.Models.Responses;
using Shared.Core.Exceptions;

namespace Modules.Storage.Core.Commands.Handlers;

public class ResolveBlobPathCommandHandler : IRequestHandler<ResolveBlobPathCommand, List<BlobProjection>>
{
    private readonly IGridFsRepository<BlobFile> _gridFsRepository;

    public ResolveBlobPathCommandHandler(IGridFsRepository<BlobFile> gridFsRepository)
    {
        _gridFsRepository = gridFsRepository;
    }

    public async Task<List<BlobProjection>> Handle(ResolveBlobPathCommand request, CancellationToken cancellationToken)
    {
        var targetList = new List<BlobProjection>();

        // Find Target File Data(First Data)
        var targetRawData = await _gridFsRepository.GetFileById(request.TargetBlobId)
                            ?? throw new ApiException(HttpStatusCode.NotFound,
                                $"Cannot find blob id: {request.TargetBlobId}");
        var targetBlobFile = targetRawData.ToBlobFile();
        var targetFileData = targetRawData.ToBlobProjection();

        // Check File Ownership
        if (targetBlobFile.OwnerId != request.UserId)
        {
            throw new ApiException(HttpStatusCode.Forbidden,
                $"Blob {request.TargetBlobId} is not user {request.UserId}'s one.");
        }

        // Add first file to list
        targetList.Add(targetFileData);

        // Define parentId
        var targetParentId = targetFileData.ParentFolderId;
        while (targetParentId != "")
        {
            var parent = (await _gridFsRepository.GetFileById(targetParentId)).ToBlobProjection();
            targetList.Add(parent);
            targetParentId = parent.ParentFolderId;
        }

        // Reverse it.
        targetList.Reverse();

        return targetList;
    }
}