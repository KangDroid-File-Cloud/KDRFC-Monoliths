using MediatR;
using Modules.Storage.Core.Models.Responses;

namespace Modules.Storage.Core.Commands;

public class GetBlobDetailCommand : IRequest<BlobProjection>
{
    public string AccountId { get; set; }
    public string BlobId { get; set; }
}