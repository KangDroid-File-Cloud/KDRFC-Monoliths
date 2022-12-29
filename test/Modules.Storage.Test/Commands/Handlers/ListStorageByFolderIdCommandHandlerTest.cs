using Modules.Storage.Core.Abstractions;
using Modules.Storage.Core.Commands;
using Modules.Storage.Core.Commands.Handlers;
using Modules.Storage.Core.Models;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Moq;
using Xunit;

namespace Modules.Storage.Test.Commands.Handlers;

public class ListStorageByFolderIdCommandHandlerTest
{
    private readonly ListStorageByFolderIdCommandHandler _handler;
    private readonly Mock<IGridFsRepository<BlobFile>> _mockGridFsRepository;

    public ListStorageByFolderIdCommandHandlerTest()
    {
        _mockGridFsRepository = new Mock<IGridFsRepository<BlobFile>>();
        _handler = new ListStorageByFolderIdCommandHandler(_mockGridFsRepository.Object);
    }

    [Fact(DisplayName = "Handle: Handle should call GridFS Repository when executed.")]
    public async Task Is_Handle_Calls_GrisFs_Repository_When_Executed()
    {
        // Let
        var command = new ListStorageByFolderIdCommand
        {
            AccountId = Ulid.NewUlid().ToString(),
            FolderId = Ulid.NewUlid().ToString()
        };
        _mockGridFsRepository.Setup(a => a.ListFileMetadataAsync(It.IsAny<FilterDefinition<GridFSFileInfo>>()))
                             .ReturnsAsync(new List<GridFSFileInfo>());

        // Do
        var response = await _handler.Handle(command, default);

        // Verify
        _mockGridFsRepository.VerifyAll();

        // Check
        Assert.Empty(response);
    }
}