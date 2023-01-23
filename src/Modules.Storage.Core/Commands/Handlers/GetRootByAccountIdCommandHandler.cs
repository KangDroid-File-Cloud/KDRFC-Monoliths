using System.Net;
using System.Text.Json;
using MediatR;
using Modules.Storage.Core.Abstractions;
using Modules.Storage.Core.Models;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Shared.Core.Commands;
using Shared.Core.Exceptions;

namespace Modules.Storage.Core.Commands.Handlers;

public class GetRootByAccountIdCommandHandler : IRequestHandler<GetRootByAccountIdCommand, string>
{
    private readonly IGridFsRepository<BlobFile> _gridFsRepository;

    public GetRootByAccountIdCommandHandler(IGridFsRepository<BlobFile> gridFsRepository)
    {
        _gridFsRepository = gridFsRepository;
    }

    public async Task<string> Handle(GetRootByAccountIdCommand request, CancellationToken cancellationToken)
    {
        var filter = Builders<GridFSFileInfo>.Filter.And(Builders<GridFSFileInfo>.Filter.Eq(
                a => a.Metadata[JsonNamingPolicy.CamelCase.ConvertName(nameof(BlobFile.ParentFolderId))], ""),
            Builders<GridFSFileInfo>.Filter.Eq(
                a => a.Metadata[JsonNamingPolicy.CamelCase.ConvertName(nameof(BlobFile.OwnerId))], request.AccountId),
            Builders<GridFSFileInfo>.Filter.Eq(
                a => a.Metadata[JsonNamingPolicy.CamelCase.ConvertName(nameof(BlobFile.BlobFileType))], BlobFileType.Folder));
        var gridFsList = await _gridFsRepository.ListFileMetadataAsync(filter);

        if (gridFsList.Count != 1)
        {
            throw new ApiException(HttpStatusCode.InternalServerError, "Cannot get root information for account!");
        }

        return gridFsList.First().Id.ToString();
    }
}