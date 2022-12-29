using System.Text.Json;
using MediatR;
using Modules.Storage.Core.Abstractions;
using Modules.Storage.Core.Extensions;
using Modules.Storage.Core.Models;
using Modules.Storage.Core.Models.Responses;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace Modules.Storage.Core.Commands.Handlers;

public class ListStorageByFolderIdCommandHandler : IRequestHandler<ListStorageByFolderIdCommand, List<BlobProjection>>
{
    private readonly IGridFsRepository<BlobFile> _gridFsRepository;

    public ListStorageByFolderIdCommandHandler(IGridFsRepository<BlobFile> gridFsRepository)
    {
        _gridFsRepository = gridFsRepository;
    }

    public async Task<List<BlobProjection>> Handle(ListStorageByFolderIdCommand request, CancellationToken cancellationToken)
    {
        var filter = Builders<GridFSFileInfo>.Filter.And(
            Builders<GridFSFileInfo>.Filter.Eq(
                a => a.Metadata[JsonNamingPolicy.CamelCase.ConvertName(nameof(BlobFile.OwnerId))], request.AccountId),
            Builders<GridFSFileInfo>.Filter.Eq(
                a => a.Metadata[JsonNamingPolicy.CamelCase.ConvertName(nameof(BlobFile.ParentFolderId))], request.FolderId));

        var fileList = await _gridFsRepository.ListFileMetadataAsync(filter);
        return fileList.Select(a => a.ToBlobProjection()).ToList();
    }
}