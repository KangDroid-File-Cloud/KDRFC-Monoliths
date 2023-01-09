using MediatR;

namespace Modules.Account.Core.Commands;

public class DropoutUserByIdCommand : IRequest
{
    public string UserId { get; set; }
    public string RootId { get; set; }
}