using MediatR;

namespace Modules.Storage.Core.Commands;

public class DeleteBlobByIdCommand : IRequest<Unit>
{
    public string AccountId { get; set; }
    public string TargetBlobId { get; set; }
}