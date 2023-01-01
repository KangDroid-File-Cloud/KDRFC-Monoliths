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

public class CreateBlobFolderCommandHandlerTest
{
    private readonly CreateBlobFolderCommandHandler _handler;
    private readonly Mock<IGridFsRepository<BlobFile>> _mockGridFsRepository;

    public CreateBlobFolderCommandHandlerTest()
    {
        _mockGridFsRepository = new Mock<IGridFsRepository<BlobFile>>();
        _handler = new CreateBlobFolderCommandHandler(_mockGridFsRepository.Object);
    }

    [Fact(DisplayName = "Handle: Handle should call repository's upload logic and find logic well.")]
    public async Task Is_Handle_Calls_Repository_Logic_Well()
    {
        // Let
        var createdFolderId = new ObjectId().ToString();
        var request = new CreateBlobFolderCommand
        {
            AccountId = Ulid.NewUlid().ToString(),
            FolderName = "KangDroidFolder",
            ParentFolderId = Ulid.NewUlid().ToString()
        };
        var testFile = new
        {
            _id = new ObjectId(createdFolderId),
            length = 100,
            uploadDate = DateTime.UtcNow,
            metadata = new BlobFile
            {
                BlobFileType = BlobFileType.Folder,
                OwnerId = request.AccountId,
                ParentFolderId = request.ParentFolderId
            }.ToBsonDocument()
        };
        _mockGridFsRepository.Setup(a => a.UploadFileAsync(request.FolderName, It.IsAny<BlobFile>(), It.IsAny<Stream>()))
                             .ReturnsAsync(createdFolderId);
        _mockGridFsRepository.Setup(a => a.GetFileById(createdFolderId))
                             .ReturnsAsync(new GridFSFileInfo(testFile.ToBsonDocument()));

        // Do
        var response = await _handler.Handle(request, default);

        // Verify
        _mockGridFsRepository.VerifyAll();

        // Check
        Assert.Equal(createdFolderId, response.Id);
        Assert.Equal(request.ParentFolderId, response.ParentFolderId);
    }

    [Fact(DisplayName =
        "Handle: Handle should throw an ApiException with Internal Server Error when file uploaded but cannot find uploaded file.")]
    public async Task Is_Handle_Throw_ApiException_With_InternalServerError_When_Cannot_Find_Uploaded_Files()
    {
        // Let
        var createdFolderId = new ObjectId().ToString();
        var request = new CreateBlobFolderCommand
        {
            AccountId = Ulid.NewUlid().ToString(),
            FolderName = "KangDroidFolder",
            ParentFolderId = Ulid.NewUlid().ToString()
        };
        var testFile = new
        {
            _id = new ObjectId(createdFolderId),
            length = 100,
            uploadDate = DateTime.UtcNow,
            metadata = new BlobFile
            {
                BlobFileType = BlobFileType.Folder,
                OwnerId = request.AccountId,
                ParentFolderId = request.ParentFolderId
            }.ToBsonDocument()
        };
        _mockGridFsRepository.Setup(a => a.UploadFileAsync(request.FolderName, It.IsAny<BlobFile>(), It.IsAny<Stream>()))
                             .ReturnsAsync(createdFolderId);
        _mockGridFsRepository.Setup(a => a.GetFileById(createdFolderId))
                             .ReturnsAsync(value: null);

        // Do
        var exception = await Assert.ThrowsAnyAsync<ApiException>(() => _handler.Handle(request, default));

        // Verify
        _mockGridFsRepository.VerifyAll();

        // Check
        Assert.Equal(StatusCodes.Status500InternalServerError, exception.StatusCode);
    }
}