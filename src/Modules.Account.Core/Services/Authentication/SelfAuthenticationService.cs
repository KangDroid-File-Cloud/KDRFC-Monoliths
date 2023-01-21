using System.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Account.Core.Abstractions;
using Modules.Account.Core.Commands;
using Modules.Account.Core.Models.Data;
using Modules.Account.Core.Models.Responses;
using Shared.Core.Abstractions;
using Shared.Core.Exceptions;
using Shared.Core.Services;

namespace Modules.Account.Core.Services.Authentication;

/// <summary>
///     Self Authentication Service designated to support Email-Password registration/login.
/// </summary>
public class SelfAuthenticationService : AuthenticationServiceBase
{
    private readonly IAccountDbContext _accountDbContext;

    public SelfAuthenticationService(IAccountDbContext accountDbContext,
                                     IJwtService jwtService, ICacheService cacheService,
                                     IMediator mediator) : base(mediator, jwtService, cacheService)
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
    public async override Task<Models.Data.Account> CreateAccountAsync(RegisterAccountCommand registerAccountCommand)
    {
        if (await _accountDbContext.Credentials.AnyAsync(
                a => a.AuthenticationProvider == AuthenticationProvider.Self && a.ProviderId == registerAccountCommand.Email))
        {
            throw new ApiException(HttpStatusCode.Conflict,
                $"Cannot register new user: {registerAccountCommand.Email} with {registerAccountCommand.AuthenticationProvider.ToString()} already exists!");
        }

        var id = Ulid.NewUlid().ToString();
        var account = new Models.Data.Account
        {
            Id = id,
            NickName = registerAccountCommand.Nickname,
            Email = registerAccountCommand.Email,
            Credentials = new List<Credential>
            {
                new()
                {
                    UserId = id,
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

    public async override Task<AccessTokenResponse> LoginAsync(LoginCommand loginCommand)
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

        // Create Login Access Token
        return await CreateLoginAccessTokenAsync(credential);
    }
}