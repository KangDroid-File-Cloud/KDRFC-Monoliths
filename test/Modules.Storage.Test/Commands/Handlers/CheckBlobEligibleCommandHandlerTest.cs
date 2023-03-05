using Microsoft.AspNetCore.Http;
using Modules.Storage.Core.Abstractions;
using Modules.Storage.Core.Commands;
using Modules.Storage.Core.Commands.Handlers;
using Modules.Storage.Core.Models;
using Modules.Storage.Core.Models.Data;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using Moq;
using Shared.Core.Abstractions;
using Shared.Core.Exceptions;
using Xunit;

namespace Modules.Storage.Test.Commands.Handlers;

public class CheckBlobEligibleCommandHandlerTest
{
    private readonly CheckBlobEligibleCommandHandler _checkBlobEligibleCommandHandler;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<IGridFsRepository<BlobFile>> _mockGridFSRepository;

    public CheckBlobEligibleCommandHandlerTest()
    {
        _mockGridFSRepository = new Mock<IGridFsRepository<BlobFile>>();
        _mockCacheService = new Mock<ICacheService>();
        _checkBlobEligibleCommandHandler =
            new CheckBlobEligibleCommandHandler(_mockGridFSRepository.Object, _mockCacheService.Object);
    }

    [Fact(DisplayName = "Handle: Handle should throw ApiException with NotFound when cannot find blob.")]
    public async Task Is_Handle_Throws_ApiException_Not_Found_When_Cannot_Find_Blob()
    {
        // Let
        var request = new CheckBlobEligibleCommand
        {
            BlobId = ObjectId.GenerateNewId().ToString(),
            UserId = Ulid.NewUlid().ToString()
        };
        _mockGridFSRepository.Setup(a => a.GetFileById(request.BlobId))
                             .ReturnsAsync(value: null);

        // Do
        var exception =
            await Assert.ThrowsAnyAsync<ApiException>(() => _checkBlobEligibleCommandHandler.Handle(request, default));

        // Verify
        _mockGridFSRepository.VerifyAll();

        // Check
        Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
    }

    [Fact(DisplayName = "Handle: Handle should throw ApiException with Forbidden when user tries to download other's blob.")]
    public async Task Is_Handle_Throws_ApiException_With_Forbidden_When_User_Tries_To_Download_Other_Blob()
    {
        // Let
        var request = new CheckBlobEligibleCommand
        {
            BlobId = ObjectId.GenerateNewId().ToString(),
            UserId = Ulid.NewUlid().ToString()
        };
        var otherBlob = new
        {
            _id = new ObjectId(),
            length = 100,
            uploadDate = DateTime.UtcNow,
            metadata = new BlobFile
            {
                BlobFileType = BlobFileType.File,
                OwnerId = Ulid.NewUlid().ToString(),
                ParentFolderId = ObjectId.GenerateNewId().ToString()
            }.ToBsonDocument()
        };
        _mockGridFSRepository.Setup(a => a.GetFileById(request.BlobId))
                             .ReturnsAsync(new GridFSFileInfo(otherBlob.ToBsonDocument()));

        // Do
        var exception =
            await Assert.ThrowsAnyAsync<ApiException>(() => _checkBlobEligibleCommandHandler.Handle(request, default));

        // Verify
        _mockGridFSRepository.VerifyAll();

        // Check
        Assert.Equal(StatusCodes.Status403Forbidden, exception.StatusCode);
    }

    [Fact(DisplayName = "Handle: Handle should throw ApiException with BadRequest when user tries to download folder.")]
    public async Task Is_Handle_Throws_ApiException_With_BadRequest_When_User_Tries_To_Download_Folder()
    {
        // Let
        var request = new CheckBlobEligibleCommand
        {
            BlobId = ObjectId.GenerateNewId().ToString(),
            UserId = Ulid.NewUlid().ToString()
        };
        var otherBlob = new
        {
            _id = new ObjectId(),
            length = 100,
            uploadDate = DateTime.UtcNow,
            metadata = new BlobFile
            {
                BlobFileType = BlobFileType.Folder,
                OwnerId = request.UserId,
                ParentFolderId = ObjectId.GenerateNewId().ToString()
            }.ToBsonDocument()
        };
        _mockGridFSRepository.Setup(a => a.GetFileById(request.BlobId))
                             .ReturnsAsync(new GridFSFileInfo(otherBlob.ToBsonDocument()));

        // Do
        var exception =
            await Assert.ThrowsAnyAsync<ApiException>(() => _checkBlobEligibleCommandHandler.Handle(request, default));

        // Verify
        _mockGridFSRepository.VerifyAll();

        // Check
        Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);
    }

    [Fact(DisplayName = "Handle: Handle should return GridFSDownloadStream when successfully handled download logic.")]
    public async Task Is_Handle_Returns_GridFSDownloadStream_When_Successfully_Handled_Download_Logic()
    {
        // Let
        var request = new CheckBlobEligibleCommand
        {
            BlobId = ObjectId.GenerateNewId().ToString(),
            UserId = Ulid.NewUlid().ToString()
        };
        var otherBlob = new
        {
            _id = new ObjectId(),
            length = 100,
            uploadDate = DateTime.UtcNow,
            metadata = new BlobFile
            {
                BlobFileType = BlobFileType.File,
                OwnerId = request.UserId,
                ParentFolderId = ObjectId.GenerateNewId().ToString()
            }.ToBsonDocument()
        };
        _mockGridFSRepository.Setup(a => a.GetFileById(request.BlobId))
                             .ReturnsAsync(new GridFSFileInfo(otherBlob.ToBsonDocument()));
        _mockCacheService.Setup(a =>
            a.SetItemAsync(StorageCacheKeys.TempBlobDownloadKey(request.BlobId), It.IsAny<object>(),
                It.IsAny<TimeSpan>()));

        // Do
        var response = await _checkBlobEligibleCommandHandler.Handle(request, default);

        // Verify
        _mockGridFSRepository.VerifyAll();
        _mockCacheService.VerifyAll();

        // Check
        Assert.NotNull(response);
    }
}