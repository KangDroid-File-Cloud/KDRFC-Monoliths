using MediatR;
using Modules.Storage.Core.Abstractions;
using Modules.Storage.Core.Models;
using MongoDB.Bson;
using Shared.Core.Commands;

namespace Modules.Storage.Core.Commands.Handlers;

public class ProvisionRootByIdCommandHandler : IRequestHandler<ProvisionRootByIdCommand, Unit>
{
    private readonly IGridFsRepository<BlobFile> _gridFsRepository;

    public ProvisionRootByIdCommandHandler(IGridFsRepository<BlobFile> gridFsRepository)
    {
        _gridFsRepository = gridFsRepository;
    }

    /// <summary>
    ///     Provision Root Folder
    /// </summary>
    /// <param name="request">Provision Request Command(Cross Module Communication)</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Provisioned Root Folder ID</returns>
    public async Task<Unit> Handle(ProvisionRootByIdCommand request, CancellationToken cancellationToken)
    {
        // Prepare Metadata
        var metadata = new BlobFile
        {
            Id = ObjectId.GenerateNewId(),
            OwnerId = request.AccountId,
            BlobFileType = BlobFileType.Folder,
            ParentFolderId = ""
        };

        // Upload to GridFS. 
        await _gridFsRepository.UploadFileAsync(request.AccountId, metadata, Stream.Null);

        return Unit.Value;
    }
}