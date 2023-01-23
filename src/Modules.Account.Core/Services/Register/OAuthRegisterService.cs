using System.Net;
using Microsoft.EntityFrameworkCore;
using Modules.Account.Core.Abstractions;
using Modules.Account.Core.Commands;
using Modules.Account.Core.Models.Data;
using Modules.Account.Core.Models.Internal;
using Shared.Core.Exceptions;

namespace Modules.Account.Core.Services.Register;

public class OAuthRegisterService : IRegisterService
{
    private readonly IAccountDbContext _accountDbContext;

    public OAuthRegisterService(IAccountDbContext accountDbContext)
    {
        _accountDbContext = accountDbContext;
    }

    public async Task<Models.Data.Account> CreateAccountAsync(RegisterAccountCommand registerAccountCommand)
    {
        // In OAuth Scenarios, authCode is JoinToken.
        var joinTokenBody = JoinTokenBody.CreateFromJwt(registerAccountCommand.AuthCode);
        if (await _accountDbContext.Credentials.AnyAsync(
                a => a.AuthenticationProvider == registerAccountCommand.AuthenticationProvider &&
                     a.ProviderId == joinTokenBody.Id))
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
                    ProviderId = joinTokenBody.Id
                }
            }
        };
        _accountDbContext.Accounts.Add(account);
        await _accountDbContext.SaveChangesAsync(default);

        return account;
    }
}