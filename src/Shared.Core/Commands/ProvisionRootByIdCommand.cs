using MediatR;

namespace Shared.Core.Commands;

public class ProvisionRootByIdCommand : IRequest<string>
{
    public string AccountId { get; set; }
}