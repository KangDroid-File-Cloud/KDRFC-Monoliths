using MediatR;
using Modules.Storage.Core.Models.Responses;

namespace Modules.Storage.Core.Commands;

public class CheckBlobEligibleCommand : IRequest<BlobEligibleResponse>
{
    public string UserId { get; set; }
    public string BlobId { get; set; }
}