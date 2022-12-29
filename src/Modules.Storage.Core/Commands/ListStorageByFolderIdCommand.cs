using MediatR;
using Modules.Storage.Core.Models.Responses;

namespace Modules.Storage.Core.Commands;

public class ListStorageByFolderIdCommand : IRequest<List<BlobProjection>>
{
    /// <summary>
    ///     Account Id(Owner)
    /// </summary>
    public string AccountId { get; set; }

    /// <summary>
    ///     Target Folder Id
    /// </summary>
    public string FolderId { get; set; }
}