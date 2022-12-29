using MediatR;

namespace Shared.Core.Commands;

public class GetRootByAccountIdCommand : IRequest<string>
{
    public string AccountId { get; set; }
}