using System.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Account.Core.Abstractions;
using Shared.Core.Exceptions;

namespace Modules.Account.Core.Commands.Handlers;

public class DropoutAccountByIdCommandHandler : IRequestHandler<DropoutUserByIdCommand, Unit>
{
    private readonly IAccountDbContext _accountDbContext;

    public DropoutAccountByIdCommandHandler(IAccountDbContext accountDbContext)
    {
        _accountDbContext = accountDbContext;
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

        return Unit.Value;
    }
}