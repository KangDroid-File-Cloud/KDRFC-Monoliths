using System.Net;
using Microsoft.EntityFrameworkCore;
using Modules.Account.Core.Abstractions;
using Modules.Account.Core.Commands;
using Modules.Account.Core.Models.Data;
using Shared.Core.Exceptions;

namespace Modules.Account.Core.Services.Authentication;

/// <summary>
///     Self Authentication Service designated to support Email-Password registration/login.
/// </summary>
public class SelfAuthenticationService : IAuthenticationService
{
    private readonly IAccountDbContext _accountDbContext;

    public SelfAuthenticationService(IAccountDbContext accountDbContext)
    {
        _accountDbContext = accountDbContext;
    }

    public async Task<Credential> AuthenticateAsync(LoginCommand loginCommand)
    {
        // Find Credential
        var credential = await _accountDbContext.Credentials
                                                .Include(a => a.Account)
                                                .Where(a => a.AuthenticationProvider == loginCommand.AuthenticationProvider &&
                                                            a.ProviderId == loginCommand.Email)
                                                .FirstOrDefaultAsync() ??
                         throw new ApiException(HttpStatusCode.Unauthorized,
                             "Login failed: Please check login information again.");

        // Self-Provider: Verify Password (TODO: Hash)
        if (credential.Key != loginCommand.AuthCode)
            throw new ApiException(HttpStatusCode.Unauthorized, "Login failed: Please check login information again.");

        return credential;
    }
}