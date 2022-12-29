using Modules.Storage.Core.Abstractions;
using Modules.Storage.Core.Commands;
using Modules.Storage.Core.Commands.Handlers;
using Modules.Storage.Core.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Moq;
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
        _mockGridFsRepository.Setup(a => a.ListFileMetadataAsync(It.IsAny<FilterDefinition<GridFSFileInfo>>()))
                             .ReturnsAsync(new List<GridFSFileInfo>
                             {
                                 new(testFile.ToBsonDocument())
                             });

        // Do
        var response = await _handler.Handle(request, default);

        // Verify
        _mockGridFsRepository.VerifyAll();

        // Check
        Assert.Equal(createdFolderId, response.Id);
        Assert.Equal(request.ParentFolderId, response.ParentFolderId);
    }
}