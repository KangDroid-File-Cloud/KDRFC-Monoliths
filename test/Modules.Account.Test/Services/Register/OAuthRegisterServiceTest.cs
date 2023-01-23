using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.JsonWebTokens;
using Modules.Account.Core.Abstractions;
using Modules.Account.Core.Commands;
using Modules.Account.Core.Models.Data;
using Modules.Account.Core.Models.Internal;
using Modules.Account.Core.Services.Register;
using Modules.Account.Infrastructure.Persistence;
using Shared.Core.Exceptions;
using Shared.Core.Services;
using Xunit;

namespace Modules.Account.Test.Services.Register;

public class OAuthRegisterServiceTest
{
    private readonly IAccountDbContext _accountDbContext;
    private readonly OAuthRegisterService _oAuthRegisterService;

    public OAuthRegisterServiceTest()
    {
        _accountDbContext = new AccountDbContext(new DbContextOptionsBuilder<AccountDbContext>()
                                                 .UseInMemoryDatabase(Ulid.NewUlid().ToString())
                                                 .Options);
        _oAuthRegisterService = new OAuthRegisterService(_accountDbContext);
    }

    private (JoinTokenBody, string) CreateJoinToken()
    {
        var jwtService =
            new JwtService(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["JwtSecurityKey"] = "testPassword@LongEnoughToEncrypt"
                }).Build(),
                NullLogger<JwtService>.Instance);
        var joinTokenBody = new JoinTokenBody
        {
            Id = Ulid.NewUlid().ToString(),
            Email = $"{Ulid.NewUlid().ToString()}@test.com",
            Provider = AuthenticationProvider.Google
        };

        // Create required claims
        var claims = new List<Claim>
        {
            new(KDRFCCommonClaimName.AuthenticationProviderId, joinTokenBody.Provider.ToString()),
            new(JwtRegisteredClaimNames.Sub, joinTokenBody.Id),
            new(KDRFCCommonClaimName.Email, joinTokenBody.Email)
        };

        return (joinTokenBody, jwtService.GenerateJwt(claims, DateTime.UtcNow.AddMinutes(10)));
    }

    [Fact(DisplayName = "CreateAccountAsync: CreateAccountAsync should throw an exception when user already exists.")]
    public async Task Is_CreateAccountAsync_Throws_ApiException_With_Conflict_When_User_Already_Exists()
    {
        // Let
        var (joinTokenBody, joinToken) = CreateJoinToken();
        var accountId = Ulid.NewUlid().ToString();
        var account = new Core.Models.Data.Account
        {
            Id = accountId,
            Email = joinTokenBody.Email,
            NickName = "KangDroid",
            Credentials = new List<Credential>
            {
                new()
                {
                    UserId = accountId,
                    AuthenticationProvider = joinTokenBody.Provider,
                    ProviderId = joinTokenBody.Id
                }
            }
        };
        _accountDbContext.Accounts.Add(account);
        await _accountDbContext.SaveChangesAsync(default);
        var registerAccountCommand = new RegisterAccountCommand
        {
            Email = joinTokenBody.Email,
            AuthenticationProvider = joinTokenBody.Provider,
            Nickname = "KangDroid",
            AuthCode = joinToken
        };

        // Do
        var exception =
            await Assert.ThrowsAnyAsync<ApiException>(() => _oAuthRegisterService.CreateAccountAsync(registerAccountCommand));

        // Check
        Assert.Equal(StatusCodes.Status409Conflict, exception.StatusCode);
    }

    [Fact(DisplayName = "CreateAccountAsync: CreateAccountAsync should add account/credential to database well.")]
    public async Task Is_CreateAccountAsync_Saves_Account_Credential_To_Database_Well()
    {
        // Let
        var (joinTokenBody, joinToken) = CreateJoinToken();
        var registerAccountCommand = new RegisterAccountCommand
        {
            Email = joinTokenBody.Email,
            AuthenticationProvider = joinTokenBody.Provider,
            Nickname = "KangDroid",
            AuthCode = joinToken
        };

        // Do
        var response = await _oAuthRegisterService.CreateAccountAsync(registerAccountCommand);

        // Check Account Entity
        Assert.Equal(registerAccountCommand.Email, response.Email);
        Assert.Equal(registerAccountCommand.Nickname, response.NickName);
        Assert.Single(response.Credentials);

        // Check Credential Entity
        var credential = response.Credentials.First();
        Assert.Equal(joinTokenBody.Provider, credential.AuthenticationProvider);
        Assert.Equal(joinTokenBody.Id, credential.ProviderId);
        Assert.Null(credential.Key);
    }
}