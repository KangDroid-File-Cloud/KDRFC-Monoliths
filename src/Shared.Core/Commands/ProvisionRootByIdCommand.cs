using MediatR;

namespace Shared.Core.Commands;

public class ProvisionRootByIdCommand : IRequest
{
    public string AccountId { get; set; }
}