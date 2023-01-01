using System.Net;
using MediatR;
using Modules.Storage.Core.Abstractions;
using Modules.Storage.Core.Extensions;
using Modules.Storage.Core.Models;
using Modules.Storage.Core.Models.Responses;
using MongoDB.Bson;
using Shared.Core.Exceptions;

namespace Modules.Storage.Core.Commands.Handlers;

public class CreateBlobFolderCommandHandler : IRequestHandler<CreateBlobFolderCommand, BlobProjection>
{
    private readonly IGridFsRepository<BlobFile> _gridFsRepository;

    public CreateBlobFolderCommandHandler(IGridFsRepository<BlobFile> gridFsRepository)
    {
        _gridFsRepository = gridFsRepository;
    }

    public async Task<BlobProjection> Handle(CreateBlobFolderCommand request, CancellationToken cancellationToken)
    {
        // Prepare Metadata
        var metadata = new BlobFile
        {
            Id = ObjectId.GenerateNewId(),
            OwnerId = request.AccountId,
            BlobFileType = BlobFileType.Folder,
            ParentFolderId = request.ParentFolderId
        };

        // Upload to GridFS.(fileId is FS's Id itself) 
        var fileId = await _gridFsRepository.UploadFileAsync(request.FolderName, metadata, Stream.Null);

        // Information
        return (await _gridFsRepository.GetFileById(fileId))?.ToBlobProjection() ??
               throw new ApiException(HttpStatusCode.InternalServerError,
                   "File uploaded but cannot find uploaded file on server!");
    }
}