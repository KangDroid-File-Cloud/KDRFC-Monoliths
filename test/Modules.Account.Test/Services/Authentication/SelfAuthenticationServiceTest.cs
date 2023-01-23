using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Modules.Account.Core.Abstractions;
using Modules.Account.Core.Commands;
using Modules.Account.Core.Models.Data;
using Modules.Account.Core.Services.Authentication;
using Modules.Account.Infrastructure.Persistence;
using Shared.Core.Exceptions;
using Xunit;

namespace Modules.Account.Test.Services.Authentication;

public class SelfAuthenticationServiceTest
{
    private readonly IAccountDbContext _accountDbContext;
    private readonly SelfAuthenticationService _selfAuthenticationService;

    public SelfAuthenticationServiceTest()
    {
        _accountDbContext = new AccountDbContext(new DbContextOptionsBuilder<AccountDbContext>()
                                                 .UseInMemoryDatabase(Ulid.NewUlid().ToString())
                                                 .Options);
        _selfAuthenticationService =
            new SelfAuthenticationService(_accountDbContext);
    }

    [Fact(DisplayName =
        "LoginAsync: LoginAsync should throw ApiException with Unauthorized Code when credential is not found.")]
    public async Task Is_LoginAsync_Throws_ApiException_With_Unauthorized_When_Credential_Not_Found()
    {
        // Let
        var loginCommand = new LoginCommand
        {
            AuthenticationProvider = AuthenticationProvider.Self,
            AuthCode = "testPassword@",
            Email = "kangdroid@test.com"
        };

        // Do
        var exception =
            await Assert.ThrowsAnyAsync<ApiException>(() => _selfAuthenticationService.AuthenticateAsync(loginCommand));

        // Check
        Assert.Equal(StatusCodes.Status401Unauthorized, exception.StatusCode);
    }

    [Fact(DisplayName =
        "LoginAsync: LoginAsync should throw ApiException with Unauthorized when credential's password does not match.")]
    public async Task Is_LoginAsync_Throws_ApiException_With_Unauthorized_When_Credential_Does_Not_Matches()
    {
        // Let
        var accountId = Ulid.NewUlid().ToString();
        var account = new Core.Models.Data.Account
        {
            Id = accountId,
            Email = "kangdroid@test.com",
            NickName = "KangDroid",
            Credentials = new List<Credential>
            {
                new()
                {
                    UserId = accountId,
                    AuthenticationProvider = AuthenticationProvider.Self,
                    ProviderId = "kangdroid@test.com",
                    Key = "testPassword@"
                }
            }
        };
        _accountDbContext.Accounts.Add(account);
        await _accountDbContext.SaveChangesAsync(default);
        var loginCommand = new LoginCommand
        {
            AuthenticationProvider = AuthenticationProvider.Self,
            AuthCode = "test",
            Email = "kangdroid@test.com"
        };

        // Do
        var exception =
            await Assert.ThrowsAnyAsync<ApiException>(() => _selfAuthenticationService.AuthenticateAsync(loginCommand));

        // Check
        Assert.Equal(StatusCodes.Status401Unauthorized, exception.StatusCode);
    }

    [Fact(DisplayName = "LoginAsync: LoginAsync should generate it jwt and refresh token well.")]
    public async Task Is_LoginAsync_Generates_Jwt_RefreshToken_Well()
    {
        // Let
        var accountId = Ulid.NewUlid().ToString();
        var account = new Core.Models.Data.Account
        {
            Id = accountId,
            Email = "kangdroid@test.com",
            NickName = "KangDroid",
            Credentials = new List<Credential>
            {
                new()
                {
                    UserId = accountId,
                    AuthenticationProvider = AuthenticationProvider.Self,
                    ProviderId = "kangdroid@test.com",
                    Key = "testPassword@"
                }
            }
        };
        _accountDbContext.Accounts.Add(account);
        await _accountDbContext.SaveChangesAsync(default);
        var loginCommand = new LoginCommand
        {
            AuthenticationProvider = AuthenticationProvider.Self,
            AuthCode = "testPassword@",
            Email = "kangdroid@test.com"
        };

        // Do
        var response = await _selfAuthenticationService.AuthenticateAsync(loginCommand);

        // Check
        var targetCredential = account.Credentials.First();
        Assert.Equal(targetCredential.UserId, response.UserId);
        Assert.Equal(targetCredential.AuthenticationProvider, response.AuthenticationProvider);
        Assert.Equal(targetCredential.ProviderId, response.ProviderId);
        Assert.Equal(targetCredential.Key, response.Key);
    }
}