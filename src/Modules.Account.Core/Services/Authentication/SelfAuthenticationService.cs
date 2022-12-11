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

    /// <summary>
    ///     See <see cref="IAuthenticationService.CreateAccountAsync" /> for top-level interface.
    /// </summary>
    /// <remarks>
    ///     Self Authentication Provider does set's user's password to Credential's "Key" property.
    ///     Note "Key" property in credential is for Self Authentication Provider only.
    /// </remarks>
    /// <param name="registerAccountCommand">Registration Request</param>
    /// <returns>Created Account Entity.</returns>
    /// <exception cref="ApiException">When user already registered within this service(409)</exception>
    public async Task<Models.Data.Account> CreateAccountAsync(RegisterAccountCommand registerAccountCommand)
    {
        if (await _accountDbContext.Credentials.AnyAsync(
                a => a.AuthenticationProvider == AuthenticationProvider.Self && a.ProviderId == registerAccountCommand.Email))
        {
            throw new ApiException(HttpStatusCode.Conflict,
                $"Cannot register new user: {registerAccountCommand.Email} with {registerAccountCommand.AuthenticationProvider.ToString()} already exists!");
        }

        var account = new Models.Data.Account
        {
            Id = Guid.NewGuid().ToString(),
            NickName = registerAccountCommand.Nickname,
            Email = registerAccountCommand.Email,
            Credentials = new List<Credential>
            {
                new()
                {
                    AuthenticationProvider = registerAccountCommand.AuthenticationProvider,
                    ProviderId = registerAccountCommand.Email,
                    Key = registerAccountCommand.AuthCode
                }
            }
        };
        _accountDbContext.Accounts.Add(account);
        await _accountDbContext.SaveChangesAsync(default);

        return account;
    }
}