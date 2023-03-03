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

public class ResolveBlobPathCommandHandlerTest
{
    private readonly Mock<IGridFsRepository<BlobFile>> _mockGridFSRepository;
    private readonly ResolveBlobPathCommandHandler _resolveBlobPathCommandHandler;

    public ResolveBlobPathCommandHandlerTest()
    {
        _mockGridFSRepository = new Mock<IGridFsRepository<BlobFile>>();
        _resolveBlobPathCommandHandler = new ResolveBlobPathCommandHandler(_mockGridFSRepository.Object);
    }

    [Fact(DisplayName = "Handle: Handle should throw ApiException with NotFound when cannot find target blob.")]
    public async Task Is_Handle_Throws_ApiException_With_NotFound_When_Cannot_Find_Blob()
    {
        // Let
        var request = new ResolveBlobPathCommand
        {
            UserId = Ulid.NewUlid().ToString(),
            TargetBlobId = ObjectId.Empty.ToString()
        };
        _mockGridFSRepository.Setup(a => a.GetFileById(request.TargetBlobId))
                             .ReturnsAsync(value: null);

        // Do
        var exception =
            await Assert.ThrowsAnyAsync<ApiException>(() => _resolveBlobPathCommandHandler.Handle(request, default));

        // Verify
        _mockGridFSRepository.VerifyAll();

        // Check
        Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
    }

    // [Fact(DisplayName =
    //     "Handle: Handle should throw ApiException with Forbidden when user tries to resolve path other's blob.")]
    // public async Task Is_Handle_Throws_ApiException_With_Forbidden_When_User_Tries_To_Resolve_Other_Blob()
    // {
    //     // Let
    //     var request = new ResolveBlobPathCommand
    //     {
    //         UserId = Ulid.NewUlid().ToString(),
    //         TargetBlobId = new ObjectId().ToString()
    //     };
    //     var parentFolder = new
    //     {
    //         _id = new ObjectId(request.TargetBlobId),
    //         length = 100,
    //         uploadDate = DateTime.UtcNow,
    //         metadata = new BlobFile
    //         {
    //             BlobFileType = BlobFileType.File,
    //             OwnerId = Ulid.NewUlid().ToString(),
    //             ParentFolderId = ""
    //         }.ToBsonDocument()
    //     };
    //     _mockGridFSRepository.Setup(a => a.GetFileById(request.TargetBlobId))
    //                          .ReturnsAsync(new GridFSFileInfo(parentFolder.ToBsonDocument()));
    //
    //     // Do
    //     var exception =
    //         await Assert.ThrowsAnyAsync<ApiException>(() => _resolveBlobPathCommandHandler.Handle(request, default));
    //
    //     // Verify
    //     _mockGridFSRepository.VerifyAll();
    //
    //     // Check
    //     Assert.Equal(StatusCodes.Status403Forbidden, exception.StatusCode);
    // }

    [Fact(DisplayName = "Handle: Handle should return list of blobFiles when successfully found full path of blob.")]
    public async Task Is_Handle_Returns_List_Of_BlobFiles_When_Successfully_Found_Full_Path()
    {
        // Let
        var userId = Ulid.NewUlid().ToString();
        var mockDataList = CreateMockData(userId);
        var request = new ResolveBlobPathCommand
        {
            TargetBlobId = mockDataList[1]._id.ToString(),
            UserId = userId
        };
        var firstObject = (object)mockDataList[1];
        _mockGridFSRepository.Setup(a => a.GetFileById(request.TargetBlobId))
                             .ReturnsAsync(new GridFSFileInfo(firstObject.ToBsonDocument()));
        var secondObject = (object)mockDataList[0];
        var secondId = mockDataList[0]._id.ToString() as string;
        _mockGridFSRepository.Setup(a => a.GetFileById(secondId!))
                             .ReturnsAsync(new GridFSFileInfo(secondObject.ToBsonDocument()));

        // Do
        var response = await _resolveBlobPathCommandHandler.Handle(request, default);

        // Verify
        _mockGridFSRepository.VerifyAll();

        // Check
        Assert.Equal(2, response.Count);
    }

    private List<dynamic> CreateMockData(string userId)
    {
        // Define Depth
        var depthOne = ObjectId.GenerateNewId(); // Root: /
        var depthTwo = ObjectId.GenerateNewId(); // Second: /blah

        return new List<dynamic>
        {
            // Root: /
            new
            {
                _id = depthOne,
                length = 100,
                uploadDate = DateTime.UtcNow,
                filename = "hello",
                metadata = new BlobFile
                {
                    BlobFileType = BlobFileType.File,
                    OwnerId = userId,
                    ParentFolderId = ""
                }.ToBsonDocument()
            },
            new
            {
                _id = depthTwo,
                length = 100,
                uploadDate = DateTime.UtcNow,
                filename = "world",
                metadata = new BlobFile
                {
                    BlobFileType = BlobFileType.File,
                    OwnerId = userId,
                    ParentFolderId = depthOne.ToString()
                }.ToBsonDocument()
            }
        };
    }
}