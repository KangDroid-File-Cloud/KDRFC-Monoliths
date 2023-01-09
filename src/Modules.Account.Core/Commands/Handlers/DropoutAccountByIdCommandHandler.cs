using System.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Account.Core.Abstractions;
using Shared.Core.Exceptions;
using Shared.Core.Notifications;

namespace Modules.Account.Core.Commands.Handlers;

public class DropoutAccountByIdCommandHandler : IRequestHandler<DropoutUserByIdCommand, Unit>
{
    private readonly IAccountDbContext _accountDbContext;
    private readonly IMediator _mediator;

    public DropoutAccountByIdCommandHandler(IAccountDbContext accountDbContext, IMediator mediator)
    {
        _accountDbContext = accountDbContext;
        _mediator = mediator;
    }

    public async Task<Unit> Handle(DropoutUserByIdCommand request, CancellationToken cancellationToken)
    {
        // Check whether user exists.
        var user = await _accountDbContext.Accounts
                                          .Include(a => a.Credentials)
                                          .Where(a => a.Id == request.UserId)
                                          .FirstOrDefaultAsync(cancellationToken)
                   ?? throw new ApiException(HttpStatusCode.NotFound, "Cannot find user!");

        // Remove Credentials(Hard Delete)
        _accountDbContext.Credentials.RemoveRange(user.Credentials);

        // Set Delete Flags
        user.IsDeleted = true;

        // Save DB
        await _accountDbContext.SaveChangesAsync(cancellationToken);

        // Send Blob Removal Request
        await _mediator.Publish(new OnRemoveBlobNotification
        {
            AccountId = request.UserId,
            TargetBlobId = request.RootId
        }, cancellationToken);

        return Unit.Value;
    }
}