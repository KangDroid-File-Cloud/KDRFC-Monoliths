using MediatR;
using Modules.Storage.Core.Models.Responses;

namespace Modules.Storage.Core.Commands;

public class CreateBlobFolderCommand : IRequest<BlobProjection>
{
    public string AccountId { get; set; }
    public string ParentFolderId { get; set; }
    public string FolderName { get; set; }
}