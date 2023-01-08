using System.Net;
using MediatR;
using Modules.Storage.Core.Abstractions;
using Modules.Storage.Core.Extensions;
using Modules.Storage.Core.Models;
using Modules.Storage.Core.Models.Responses;
using MongoDB.Bson;
using Shared.Core.Exceptions;

namespace Modules.Storage.Core.Commands.Handlers;

public class CreateBlobFileCommandHandler : IRequestHandler<CreateBlobFileCommand, BlobProjection>
{
    private readonly IGridFsRepository<BlobFile> _gridFsRepository;

    public CreateBlobFileCommandHandler(IGridFsRepository<BlobFile> gridFsRepository)
    {
        _gridFsRepository = gridFsRepository;
    }

    public async Task<BlobProjection> Handle(CreateBlobFileCommand request, CancellationToken cancellationToken)
    {
        var parentFileInfo = await _gridFsRepository.GetFileById(request.ParentFolderId)
                             ?? throw new ApiException(HttpStatusCode.NotFound,
                                 $"Cannot find folder with id: {request.ParentFolderId}");
        var parentFileBlob = parentFileInfo.ToBlobFile();

        // if blob file is not a folder, return 400 bad request.
        if (parentFileBlob.BlobFileType != BlobFileType.Folder)
            throw new ApiException(HttpStatusCode.BadRequest, $"Blob ID {request.ParentFolderId} is NOT a folder!");

        // if blob file is not owned by user, return 403 forbidden.
        if (parentFileBlob.OwnerId != request.AccountId)
        {
            throw new ApiException(HttpStatusCode.Forbidden,
                $"Parent file ID {parentFileInfo.Id.ToString()} is not user's one!");
        }

        // Prepare Metadata
        var metadata = new BlobFile
        {
            Id = ObjectId.GenerateNewId(),
            OwnerId = request.AccountId,
            BlobFileType = BlobFileType.File,
            ParentFolderId = parentFileInfo.Id.ToString()
        };

        // Upload to GridFS.(fileId is FS's Id itself) 
        var fileId = await _gridFsRepository.UploadFileAsync(request.FileName, metadata, request.FileContent);

        // Information
        return (await _gridFsRepository.GetFileById(fileId))?.ToBlobProjection() ??
               throw new ApiException(HttpStatusCode.InternalServerError,
                   "File uploaded but cannot find uploaded file on server!");
    }
}