using MediatR;
using Microsoft.AspNetCore.Http;
using Modules.Storage.Core.Abstractions;
using Modules.Storage.Core.Commands;
using Modules.Storage.Core.Commands.Handlers;
using Modules.Storage.Core.Models;
using Modules.Storage.Core.Notifications;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using Moq;
using Shared.Core.Exceptions;
using Xunit;

namespace Modules.Storage.Test.Commands.Handlers;

public class DeleteBlobByIdCommandHandlerTest
{
    private readonly DeleteBlobByIdCommandHandler _handler;
    private readonly Mock<IGridFsRepository<BlobFile>> _mockGridFsRepository;
    private readonly Mock<IMediator> _mockMediator;

    public DeleteBlobByIdCommandHandlerTest()
    {
        _mockGridFsRepository = new Mock<IGridFsRepository<BlobFile>>();
        _mockMediator = new Mock<IMediator>();
        _handler = new DeleteBlobByIdCommandHandler(_mockGridFsRepository.Object, _mockMediator.Object);
    }

    [Fact(DisplayName = "Handle: Handle should throw ApiException with NotFound when target blob file not found.")]
    public async Task Is_Handle_Throws_ApiException_With_NotFound_When_Target_Blob_Not_Found()
    {
        // Let
        var request = new DeleteBlobByIdCommand
        {
            AccountId = Ulid.NewUlid().ToString(),
            TargetBlobId = ObjectId.GenerateNewId().ToString()
        };
        _mockGridFsRepository.Setup(a => a.GetFileById(request.TargetBlobId))
                             .ReturnsAsync(value: null);

        // Do
        var exception = await Assert.ThrowsAnyAsync<ApiException>(() => _handler.Handle(request, default));

        // Verify
        _mockGridFsRepository.VerifyAll();

        // Check Exception
        Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
    }

    [Fact(DisplayName = "Handle: Handle should throw ApiException with Forbidden when target blob is not user's one.")]
    public async Task Is_Handle_Throws_ApiException_With_Forbidden_When_Target_Blob_Not_Owned_By_User()
    {
        var blobFile = new
        {
            _id = ObjectId.GenerateNewId(),
            length = 100,
            uploadDate = DateTime.UtcNow,
            metadata = new BlobFile
            {
                BlobFileType = BlobFileType.Folder,
                OwnerId = Ulid.NewUlid().ToString(),
                ParentFolderId = ObjectId.GenerateNewId().ToString()
            }.ToBsonDocument()
        };
        var command = new DeleteBlobByIdCommand
        {
            AccountId = Ulid.NewUlid().ToString(),
            TargetBlobId = blobFile._id.ToString()
        };
        _mockGridFsRepository.Setup(a => a.GetFileById(command.TargetBlobId))
                             .ReturnsAsync(new GridFSFileInfo(blobFile.ToBsonDocument()));

        // Do
        var exception = await Assert.ThrowsAnyAsync<ApiException>(() => _handler.Handle(command, default));

        // Verify
        _mockGridFsRepository.VerifyAll();

        // Check Exception
        Assert.Equal(StatusCodes.Status403Forbidden, exception.StatusCode);
    }

    [Fact(DisplayName = "Handle: Handle should call mediator's publish method when request is valid.")]
    public async Task Is_Handle_Call_Mediator_Publish_Method_When_Request_Valid()
    {
        var accountId = Ulid.NewUlid().ToString();
        var blobFile = new
        {
            _id = ObjectId.GenerateNewId(),
            length = 100,
            uploadDate = DateTime.UtcNow,
            metadata = new BlobFile
            {
                BlobFileType = BlobFileType.Folder,
                OwnerId = accountId,
                ParentFolderId = ObjectId.GenerateNewId().ToString()
            }.ToBsonDocument()
        };
        var command = new DeleteBlobByIdCommand
        {
            AccountId = accountId,
            TargetBlobId = blobFile._id.ToString()
        };
        _mockGridFsRepository.Setup(a => a.GetFileById(command.TargetBlobId))
                             .ReturnsAsync(new GridFSFileInfo(blobFile.ToBsonDocument()));
        _mockMediator.Setup(a => a.Publish(It.IsAny<OnRemoveBlobNotification>(), It.IsAny<CancellationToken>()))
                     .Callback((OnRemoveBlobNotification notification, CancellationToken token) =>
                     {
                         Assert.Equal(accountId, notification.AccountId);
                         Assert.Equal(command.TargetBlobId, notification.TargetBlobId);
                     });

        // Do
        await _handler.Handle(command, default);

        // Verify
        _mockGridFsRepository.VerifyAll();
        _mockMediator.VerifyAll();
    }
}