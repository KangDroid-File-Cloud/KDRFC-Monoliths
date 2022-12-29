using Modules.Storage.Core.Abstractions;
using Modules.Storage.Core.Commands.Handlers;
using Modules.Storage.Core.Models;
using MongoDB.Bson;
using Moq;
using Shared.Core.Commands;
using Xunit;

namespace Modules.Storage.Test.Commands.Handlers;

public class ProvisionRootByCommandHandlerTest
{
    private readonly ProvisionRootByIdCommandHandler _handler;
    private readonly Mock<IGridFsRepository<BlobFile>> _mockGridFsRepository;

    public ProvisionRootByCommandHandlerTest()
    {
        _mockGridFsRepository = new Mock<IGridFsRepository<BlobFile>>();
        _handler = new ProvisionRootByIdCommandHandler(_mockGridFsRepository.Object);
    }

    [Fact(DisplayName = "Handle: Handle should upload file metadata to repository when handler received request.")]
    public async Task Is_Handle_Upload_Metadata_To_Repository_When_Request_Received()
    {
        // Let
        var request = new ProvisionRootByIdCommand
        {
            AccountId = Ulid.NewUlid().ToString()
        };
        _mockGridFsRepository.Setup(a => a.UploadFileAsync(request.AccountId, It.IsAny<BlobFile>(), It.IsAny<Stream>()))
                             .Callback((string fileName, BlobFile metadata, Stream stream) =>
                             {
                                 Assert.Equal(request.AccountId, fileName);
                                 Assert.NotEqual(ObjectId.Empty, metadata.Id);
                                 Assert.Equal(request.AccountId, metadata.OwnerId);
                                 Assert.Equal(BlobFileType.Folder, metadata.BlobFileType);
                                 Assert.Empty(metadata.ParentFolderId);
                             })
                             .ReturnsAsync("fileId");

        // Do
        await _handler.Handle(request, default);

        // Verify
        _mockGridFsRepository.VerifyAll();
    }
}