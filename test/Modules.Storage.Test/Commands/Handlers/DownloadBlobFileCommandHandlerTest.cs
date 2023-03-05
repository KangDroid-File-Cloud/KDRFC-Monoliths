using Microsoft.AspNetCore.Http;
using Modules.Storage.Core.Abstractions;
using Modules.Storage.Core.Commands;
using Modules.Storage.Core.Commands.Handlers;
using Modules.Storage.Core.Models;
using Modules.Storage.Core.Models.Data;
using Modules.Storage.Core.Models.Responses;
using MongoDB.Bson;
using Moq;
using Shared.Core.Abstractions;
using Shared.Core.Exceptions;
using Xunit;

namespace Modules.Storage.Test.Commands.Handlers;

public class DownloadBlobFileCommandHandlerTest
{
    private readonly DownloadBlobFileCommandHandler _downloadBlobFileCommandHandler;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<IGridFsRepository<BlobFile>> _mockGridFSRepository;

    public DownloadBlobFileCommandHandlerTest()
    {
        _mockGridFSRepository = new Mock<IGridFsRepository<BlobFile>>();
        _mockCacheService = new Mock<ICacheService>();
        _downloadBlobFileCommandHandler =
            new DownloadBlobFileCommandHandler(_mockGridFSRepository.Object, _mockCacheService.Object);
    }

    [Fact(DisplayName = "Handle: handle should throw an ApiException when cannot find cache keys.")]
    public async Task Is_Handle_Throws_ApiException_With_Unauthorized_When_Cannot_Find_Cache_Keys()
    {
        // Let
        var request = new DownloadBlobFileCommand
        {
            BlobId = ObjectId.GenerateNewId().ToString(),
            BlobAccessToken = Ulid.NewUlid().ToString()
        };
        _mockCacheService
            .Setup(a => a.GetItemAsync<BlobEligibleResponse>(StorageCacheKeys.TempBlobDownloadKey(request.BlobId)))
            .ReturnsAsync(value: null);

        // Do
        var exception =
            await Assert.ThrowsAnyAsync<ApiException>(() => _downloadBlobFileCommandHandler.Handle(request, default));

        // Verify
        _mockCacheService.VerifyAll();

        // Check
        Assert.Equal(StatusCodes.Status401Unauthorized, exception.StatusCode);
    }

    [Fact(DisplayName = "Handle: Handle should throw an ApiException with 401 Unauthorized when blob accesstoken is differ.")]
    public async Task Is_Handle_Throws_ApiException_With_Unauthorized_When_Blob_AccessToken_Differ()
    {
        // Let
        var request = new DownloadBlobFileCommand
        {
            BlobId = ObjectId.GenerateNewId().ToString(),
            BlobAccessToken = Ulid.NewUlid().ToString()
        };
        _mockCacheService
            .Setup(a => a.GetItemAsync<BlobEligibleResponse>(StorageCacheKeys.TempBlobDownloadKey(request.BlobId)))
            .ReturnsAsync(new BlobEligibleResponse
            {
                Token = Ulid.NewUlid().ToString()
            });

        // Do
        var exception =
            await Assert.ThrowsAnyAsync<ApiException>(() => _downloadBlobFileCommandHandler.Handle(request, default));

        // Verify
        _mockCacheService.VerifyAll();

        // Check
        Assert.Equal(StatusCodes.Status401Unauthorized, exception.StatusCode);
    }

    [Fact(DisplayName = "Handle: Handle should return GridFSDownloadStream when successfully handled download logic.")]
    public async Task Is_Handle_Returns_GridFSDownloadStream_When_Successfully_Handled_Download_Logic()
    {
        // Let
        var request = new DownloadBlobFileCommand
        {
            BlobId = ObjectId.GenerateNewId().ToString(),
            BlobAccessToken = Ulid.NewUlid().ToString()
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
        _mockCacheService
            .Setup(a => a.GetItemAsync<BlobEligibleResponse>(StorageCacheKeys.TempBlobDownloadKey(request.BlobId)))
            .ReturnsAsync(new BlobEligibleResponse
            {
                Token = request.BlobAccessToken
            });
        _mockGridFSRepository.Setup(a => a.OpenDownloadStreamAsync(request.BlobId, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(value: null); // Cannot mock Download Stream, so just null check.

        // Do
        var response = await _downloadBlobFileCommandHandler.Handle(request, default);

        // Verify
        _mockGridFSRepository.VerifyAll();
        _mockCacheService.VerifyAll();

        // Check
        Assert.Null(response);
    }
}