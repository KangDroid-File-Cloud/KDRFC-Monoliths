using Microsoft.AspNetCore.Http;
using Modules.Storage.Core.Abstractions;
using Modules.Storage.Core.Commands;
using Modules.Storage.Core.Commands.Handlers;
using Modules.Storage.Core.Models;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using Moq;
using Shared.Core.Exceptions;
using Shared.Test.Extensions;
using Xunit;

namespace Modules.Storage.Test.Commands.Handlers;

public class CreateBlobFileCommandHandlerTest
{
    private readonly CreateBlobFileCommandHandler _createBlobFileCommandHandler;
    private readonly Mock<IGridFsRepository<BlobFile>> _mockGridFSRepository;

    public CreateBlobFileCommandHandlerTest()
    {
        _mockGridFSRepository = new Mock<IGridFsRepository<BlobFile>>();
        _createBlobFileCommandHandler = new CreateBlobFileCommandHandler(_mockGridFSRepository.Object);
    }

    [Fact(DisplayName = "Handle: Handle should throw an API Exception with NotFound when cannot find parent folder id.")]
    public async Task Is_Handle_Throws_ApiException_With_NotFound_When_ParentFolder_Not_Found()
    {
        // Let
        var request = new CreateBlobFileCommand
        {
            ParentFolderId = ObjectId.GenerateNewId().ToString()
        };
        _mockGridFSRepository.Setup(a => a.GetFileById(request.ParentFolderId))
                             .ReturnsAsync(value: null);

        // Do
        var exception =
            await Assert.ThrowsAnyAsync<ApiException>(() => _createBlobFileCommandHandler.Handle(request, default));

        // Verify
        _mockGridFSRepository.VerifyAll();

        // Check
        Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
    }

    [Fact(DisplayName =
        "Handle: Handle should throw an ApiException with BadRequest when parentFolderId is not actual folder.")]
    public async Task Is_Handle_Throws_ApiException_When_ParentFolder_Not_Folder()
    {
        // Let
        var request = new CreateBlobFileCommand
        {
            ParentFolderId = ObjectId.GenerateNewId().ToString()
        };
        var parentFolder = new
        {
            _id = new ObjectId(request.ParentFolderId),
            length = 100,
            uploadDate = DateTime.UtcNow,
            metadata = new BlobFile
            {
                BlobFileType = BlobFileType.File,
                OwnerId = request.AccountId,
                ParentFolderId = request.ParentFolderId
            }.ToBsonDocument()
        };
        _mockGridFSRepository.Setup(a => a.GetFileById(request.ParentFolderId))
                             .ReturnsAsync(new GridFSFileInfo(parentFolder.ToBsonDocument()));

        // Do
        var exception =
            await Assert.ThrowsAnyAsync<ApiException>(() => _createBlobFileCommandHandler.Handle(request, default));

        // Verify
        _mockGridFSRepository.VerifyAll();

        // Check
        Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);
    }

    [Fact(DisplayName = "Handle: Handle should throw an ApiException with Forbidden when parent folder is not owned by user.")]
    public async Task Is_Handle_Throw_An_ApiException_With_Forbidden_When_ParentFolder_Not_Owned_By_User()
    {
        // Let
        var request = new CreateBlobFileCommand
        {
            ParentFolderId = ObjectId.GenerateNewId().ToString()
        };
        var parentFolder = new
        {
            _id = new ObjectId(request.ParentFolderId),
            length = 100,
            uploadDate = DateTime.UtcNow,
            metadata = new BlobFile
            {
                BlobFileType = BlobFileType.Folder,
                OwnerId = Ulid.NewUlid().ToString(),
                ParentFolderId = request.ParentFolderId
            }.ToBsonDocument()
        };
        _mockGridFSRepository.Setup(a => a.GetFileById(request.ParentFolderId))
                             .ReturnsAsync(new GridFSFileInfo(parentFolder.ToBsonDocument()));

        // Do
        var exception =
            await Assert.ThrowsAnyAsync<ApiException>(() => _createBlobFileCommandHandler.Handle(request, default));

        // Verify
        _mockGridFSRepository.VerifyAll();

        // Check
        Assert.Equal(StatusCodes.Status403Forbidden, exception.StatusCode);
    }

    [Fact(DisplayName =
        "Handle: Handle should throw an ApiException with InternalServerError when file uploaded by cannot find by id.")]
    public async Task Is_Handle_Throw_ApiException_With_InternalServerError_When_File_Uploaded_But_Cannot_Find()
    {
        // Let
        var request = new CreateBlobFileCommand
        {
            ParentFolderId = ObjectId.GenerateNewId().ToString(),
            AccountId = Ulid.NewUlid().ToString(),
            FileContent = "test".CreateStream(),
            FileName = "hello.txt"
        };
        var parentFolder = new
        {
            _id = new ObjectId(request.ParentFolderId),
            length = 100,
            uploadDate = DateTime.UtcNow,
            metadata = new BlobFile
            {
                BlobFileType = BlobFileType.Folder,
                OwnerId = request.AccountId,
                ParentFolderId = request.ParentFolderId
            }.ToBsonDocument()
        };
        var uploadedId = ObjectId.GenerateNewId().ToString();
        _mockGridFSRepository.Setup(a => a.GetFileById(request.ParentFolderId))
                             .ReturnsAsync(new GridFSFileInfo(parentFolder.ToBsonDocument()));
        _mockGridFSRepository.Setup(a => a.UploadFileAsync(request.FileName, It.IsAny<BlobFile>(), request.FileContent))
                             .ReturnsAsync(uploadedId);
        _mockGridFSRepository.Setup(a => a.GetFileById(uploadedId))
                             .ReturnsAsync(value: null);

        // Do
        var exception =
            await Assert.ThrowsAnyAsync<ApiException>(() => _createBlobFileCommandHandler.Handle(request, default));

        // Verify
        _mockGridFSRepository.VerifyAll();

        // Check
        Assert.Equal(StatusCodes.Status500InternalServerError, exception.StatusCode);
    }

    [Fact(DisplayName = "Handle: Handle should return 200 OK When request are all valid.")]
    public async Task Is_Handle_Returns_200_Ok_When_Request_Valid()
    {
        // Let
        var request = new CreateBlobFileCommand
        {
            ParentFolderId = ObjectId.GenerateNewId().ToString(),
            AccountId = Ulid.NewUlid().ToString(),
            FileContent = "test".CreateStream(),
            FileName = "hello.txt"
        };
        var parentFolder = new
        {
            _id = new ObjectId(request.ParentFolderId),
            length = 100,
            uploadDate = DateTime.UtcNow,
            metadata = new BlobFile
            {
                BlobFileType = BlobFileType.Folder,
                OwnerId = request.AccountId,
                ParentFolderId = request.ParentFolderId
            }.ToBsonDocument()
        };
        var uploadedId = ObjectId.GenerateNewId().ToString();
        var uploadFile = new
        {
            _id = new ObjectId(uploadedId),
            length = 100,
            uploadDate = DateTime.UtcNow,
            metadata = new BlobFile
            {
                BlobFileType = BlobFileType.File,
                OwnerId = request.AccountId,
                ParentFolderId = request.ParentFolderId
            }.ToBsonDocument()
        };
        _mockGridFSRepository.Setup(a => a.GetFileById(request.ParentFolderId))
                             .ReturnsAsync(new GridFSFileInfo(parentFolder.ToBsonDocument()));
        _mockGridFSRepository.Setup(a => a.UploadFileAsync(request.FileName, It.IsAny<BlobFile>(), request.FileContent))
                             .ReturnsAsync(uploadedId);
        _mockGridFSRepository.Setup(a => a.GetFileById(uploadedId))
                             .ReturnsAsync(new GridFSFileInfo(uploadFile.ToBsonDocument()));

        // Do
        var response = await _createBlobFileCommandHandler.Handle(request, default);

        // Verify
        _mockGridFSRepository.VerifyAll();

        // Check
        Assert.Equal(uploadedId, response.Id);
        Assert.Equal(parentFolder._id.ToString(), response.ParentFolderId);
        Assert.Equal(uploadFile.length, response.Length);
        Assert.Equal(BlobFileType.File, response.BlobFileType);
    }
}