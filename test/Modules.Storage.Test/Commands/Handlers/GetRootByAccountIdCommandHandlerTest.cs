using Microsoft.AspNetCore.Http;
using Modules.Storage.Core.Abstractions;
using Modules.Storage.Core.Commands.Handlers;
using Modules.Storage.Core.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Moq;
using Shared.Core.Commands;
using Shared.Core.Exceptions;
using Xunit;

namespace Modules.Storage.Test.Commands.Handlers;

public class GetRootByAccountIdCommandHandlerTest
{
    private readonly GetRootByAccountIdCommandHandler _handler;
    private readonly Mock<IGridFsRepository<BlobFile>> _mockGridFsRepository;

    public GetRootByAccountIdCommandHandlerTest()
    {
        _mockGridFsRepository = new Mock<IGridFsRepository<BlobFile>>();
        _handler = new GetRootByAccountIdCommandHandler(_mockGridFsRepository.Object);
    }

    [Fact(DisplayName = "Handle: Handle should throw ApiException with InternalServerError Status Code root is not found.")]
    public async Task Is_Handle_Throws_ApiException_With_InternalServerError_When_File_Not_Found()
    {
        // Let
        var request = new GetRootByAccountIdCommand
        {
            AccountId = Ulid.NewUlid().ToString()
        };
        _mockGridFsRepository.Setup(a => a.ListFileMetadataAsync(It.IsAny<FilterDefinition<GridFSFileInfo>>()))
                             .ReturnsAsync(new List<GridFSFileInfo>());

        // Do
        var exception = await Assert.ThrowsAnyAsync<ApiException>(() => _handler.Handle(request, default));

        // Check
        Assert.Equal(StatusCodes.Status500InternalServerError, exception.StatusCode);
    }

    [Fact(DisplayName = "Handle: Handle should return document Id when user's root id exists.")]
    public async Task Is_Handle_Returns_User_Root_Folder_Id_When_Root_Exists()
    {
        // Let
        var request = new GetRootByAccountIdCommand
        {
            AccountId = Ulid.NewUlid().ToString()
        };
        var file = new
        {
            _id = ObjectId.Empty
        };
        _mockGridFsRepository.Setup(a => a.ListFileMetadataAsync(It.IsAny<FilterDefinition<GridFSFileInfo>>()))
                             .ReturnsAsync(new List<GridFSFileInfo>
                             {
                                 new(file.ToBsonDocument())
                             });

        // Do
        var response = await _handler.Handle(request, default);

        // Check
        Assert.Equal(file._id.ToString(), response);
    }
}