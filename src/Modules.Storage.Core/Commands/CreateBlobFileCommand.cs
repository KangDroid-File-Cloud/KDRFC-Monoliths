using MediatR;
using Modules.Storage.Core.Models.Responses;

namespace Modules.Storage.Core.Commands;

public class CreateBlobFileCommand : IRequest<BlobProjection>
{
    public string AccountId { get; set; }
    public string ParentFolderId { get; set; }
    public string FileName { get; set; }
    public string FileMimeType { get; set; }
    public Stream FileContent { get; set; }
}