using Microsoft.AspNetCore.Http;
using Modules.Storage.Core.Abstractions;
using Modules.Storage.Core.Commands;
using Modules.Storage.Core.Commands.Handlers;
using Modules.Storage.Core.Models;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using Moq;
using Shared.Core.Exceptions;
using Xunit;

namespace Modules.Storage.Test.Commands.Handlers;

public class GetBlobDetailCommandHandlerTest
{
    private readonly GetBlobDetailCommandHandler _blobDetailCommandHandler;
    private readonly Mock<IGridFsRepository<BlobFile>> _mockGridFsRepository;

    public GetBlobDetailCommandHandlerTest()
    {
        _mockGridFsRepository = new Mock<IGridFsRepository<BlobFile>>();
        _blobDetailCommandHandler = new GetBlobDetailCommandHandler(_mockGridFsRepository.Object);
    }

    [Fact(DisplayName = "Handle: Handle should throw ApiException with 404 NotFound when blob information not found.")]
    public async Task Is_Handle_Throws_ApiException_With_404_NotFound_When_Blob_Not_Found()
    {
        // Let
        var request = new GetBlobDetailCommand
        {
            BlobId = ObjectId.Empty.ToString(),
            AccountId = Ulid.NewUlid().ToString()
        };
        _mockGridFsRepository.Setup(a => a.GetFileById(request.BlobId))
                             .ReturnsAsync(value: null);

        // Do
        var exception = await Assert.ThrowsAnyAsync<ApiException>(() => _blobDetailCommandHandler.Handle(request, default));

        // Verify
        _mockGridFsRepository.VerifyAll();

        // Check
        Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
    }

    [Fact(DisplayName =
        "Handle: Handle should throw ApiException with 403 Forbidden when blob is not owned by requested user.")]
    public async Task Is_Handle_Returns_Forbidden_When_Blob_Not_Owned_By_Requested_User()
    {
        // Let
        var request = new GetBlobDetailCommand
        {
            BlobId = ObjectId.Empty.ToString(),
            AccountId = Ulid.NewUlid().ToString()
        };
        var parentFolder = new
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
        _mockGridFsRepository.Setup(a => a.GetFileById(request.BlobId))
                             .ReturnsAsync(new GridFSFileInfo(parentFolder.ToBsonDocument()));

        // Do
        var exception = await Assert.ThrowsAnyAsync<ApiException>(() => _blobDetailCommandHandler.Handle(request, default));

        // Verify
        _mockGridFsRepository.VerifyAll();

        // Check
        Assert.Equal(StatusCodes.Status403Forbidden, exception.StatusCode);
    }

    [Fact(DisplayName = "Handle: Handle should return BlobProjection when all requests are valid.")]
    public async Task Is_Handle_Return_BlobProjection_When_Request_Valid()
    {
        // Let
        var request = new GetBlobDetailCommand
        {
            BlobId = ObjectId.Empty.ToString(),
            AccountId = Ulid.NewUlid().ToString()
        };
        var parentFolder = new
        {
            _id = ObjectId.GenerateNewId(),
            length = 100,
            uploadDate = DateTime.UtcNow,
            metadata = new BlobFile
            {
                BlobFileType = BlobFileType.Folder,
                OwnerId = request.AccountId,
                ParentFolderId = ObjectId.GenerateNewId().ToString()
            }.ToBsonDocument()
        };
        _mockGridFsRepository.Setup(a => a.GetFileById(request.BlobId))
                             .ReturnsAsync(new GridFSFileInfo(parentFolder.ToBsonDocument()));

        // Do
        var blobProjection = await _blobDetailCommandHandler.Handle(request, default);

        // Verify
        _mockGridFsRepository.VerifyAll();

        // Check
        Assert.Equal(parentFolder._id.ToString(), blobProjection.Id);
        Assert.Equal(parentFolder.length, blobProjection.Length);
        Assert.Equal(parentFolder.metadata["parentFolderId"], blobProjection.ParentFolderId);
    }
}