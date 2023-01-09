using System.Net;
using MediatR;
using Modules.Storage.Core.Abstractions;
using Modules.Storage.Core.Extensions;
using Modules.Storage.Core.Models;
using Shared.Core.Exceptions;
using Shared.Core.Notifications;

namespace Modules.Storage.Core.Commands.Handlers;

public class DeleteBlobByIdCommandHandler : IRequestHandler<DeleteBlobByIdCommand, Unit>
{
    private readonly IGridFsRepository<BlobFile> _gridFsRepository;
    private readonly IMediator _mediator;

    public DeleteBlobByIdCommandHandler(IGridFsRepository<BlobFile> gridFsRepository, IMediator mediator)
    {
        _gridFsRepository = gridFsRepository;
        _mediator = mediator;
    }

    public async Task<Unit> Handle(DeleteBlobByIdCommand request, CancellationToken cancellationToken)
    {
        var blobFile = await _gridFsRepository.GetFileById(request.TargetBlobId)
                       ?? throw new ApiException(HttpStatusCode.NotFound,
                           $"Cannot find Blob file with id: {request.TargetBlobId}");
        var parentFileBlob = blobFile.ToBlobFile();

        // if blob file is not owned by user, return 403 forbidden.
        if (parentFileBlob.OwnerId != request.AccountId)
        {
            throw new ApiException(HttpStatusCode.Forbidden,
                $"Parent file ID {parentFileBlob.Id.ToString()} is not user's one!");
        }

        // Publish Notification(Long Running Tasks)
        await _mediator.Publish(new OnRemoveBlobNotification
        {
            AccountId = request.AccountId,
            TargetBlobId = request.TargetBlobId
        }, cancellationToken);

        return Unit.Value;
    }
}