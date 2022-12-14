using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Modules.Account.Core.Abstractions;
using Modules.Account.Core.Commands;
using Modules.Account.Core.Models.Data;
using Modules.Account.Core.Services.Authentication;
using Modules.Account.Infrastructure.Persistence;
using Moq;
using Shared.Core.Abstractions;
using Shared.Core.Exceptions;
using Shared.Core.Services;
using Xunit;

namespace Modules.Account.Test.Services.Authentication;

public class SelfAuthenticationServiceTest
{
    private readonly IAccountDbContext _accountDbContext;
    private readonly Mock<ICacheService> _mockCacheService;

    private readonly Mock<IJwtService> _mockJwtService;
    private readonly SelfAuthenticationService _selfAuthenticationService;

    public SelfAuthenticationServiceTest()
    {
        _accountDbContext = new AccountDbContext(new DbContextOptionsBuilder<AccountDbContext>()
                                                 .UseInMemoryDatabase(Guid.NewGuid().ToString())
                                                 .Options);
        _mockJwtService = new Mock<IJwtService>();
        _mockCacheService = new Mock<ICacheService>();
        _selfAuthenticationService =
            new SelfAuthenticationService(_accountDbContext, _mockJwtService.Object, _mockCacheService.Object);
    }

    [Fact(DisplayName = "CreateAccountAsync: CreateAccountAsync should register user to database when request is valid.")]
    public async Task Is_CreateAccountAsync_Registers_New_User()
    {
        // Let
        var request = new RegisterAccountCommand
        {
            AuthenticationProvider = AuthenticationProvider.Self,
            Email = "kangdroid@test.com",
            Nickname = "kangdroid",
            AuthCode = "testPassword@"
        };

        // Do
        await _selfAuthenticationService.CreateAccountAsync(request);

        // Check we've got single account.
        var accounts = await _accountDbContext.Accounts.Include(a => a.Credentials)
                                              .ToListAsync();
        Assert.Single(accounts);

        // Check Account 
        var account = accounts.First();
        Assert.Equal(request.Nickname, account.NickName);
        Assert.Equal(request.Email, account.Email);

        // Check Credentials
        Assert.Single(account.Credentials);
        var credential = account.Credentials.First();
        Assert.Equal(AuthenticationProvider.Self, credential.AuthenticationProvider);
        Assert.Equal(request.Email, credential.ProviderId);
        Assert.Equal(request.AuthCode, credential.Key);
    }

    [Fact(DisplayName =
        "CreateAccountAsync: CreateAccountAsync should throw ApiException with 409 when registered account already exists.")]
    public async Task Is_CreateAccountAsync_Throws_ApiException_When_Already_Registered()
    {
        // Let
        var accountId = Guid.NewGuid().ToString();
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

        var request = new RegisterAccountCommand
        {
            AuthenticationProvider = AuthenticationProvider.Self,
            Email = "kangdroid@test.com",
            Nickname = "kangdroid",
            AuthCode = "testPassword@"
        };

        // Do
        var exception =
            await Assert.ThrowsAnyAsync<ApiException>(() => _selfAuthenticationService.CreateAccountAsync(request));

        // Check Exception's Value
        Assert.Equal(StatusCodes.Status409Conflict, exception.StatusCode);
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
        var exception = await Assert.ThrowsAnyAsync<ApiException>(() => _selfAuthenticationService.LoginAsync(loginCommand));

        // Check
        Assert.Equal(StatusCodes.Status401Unauthorized, exception.StatusCode);
    }

    [Fact(DisplayName =
        "LoginAsync: LoginAsync should throw ApiException with Unauthorized when credential's password does not match.")]
    public async Task Is_LoginAsync_Throws_ApiException_With_Unauthorized_When_Credential_Does_Not_Matches()
    {
        // Let
        var accountId = Guid.NewGuid().ToString();
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
        var exception = await Assert.ThrowsAnyAsync<ApiException>(() => _selfAuthenticationService.LoginAsync(loginCommand));

        // Check
        Assert.Equal(StatusCodes.Status401Unauthorized, exception.StatusCode);
    }

    [Fact(DisplayName = "LoginAsync: LoginAsync should generate it jwt and refresh token well.")]
    public async Task Is_LoginAsync_Generates_Jwt_RefreshToken_Well()
    {
        // Let
        var accountId = Guid.NewGuid().ToString();
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
        _mockJwtService.Setup(a => a.GenerateJwt(It.IsAny<List<Claim>>(), null))
                       .Returns("testJwt");
        _mockJwtService.Setup(a => a.GenerateJwt(null, It.IsAny<DateTime>()))
                       .Returns("RefreshTokenTest");
        _mockCacheService.Setup(a =>
            a.SetItemAsync(AccountCacheKeys.RefreshTokenKey("RefreshTokenTest"), It.IsAny<object>(), It.IsAny<TimeSpan>()));

        // Do
        var response = await _selfAuthenticationService.LoginAsync(loginCommand);

        // Verify
        _mockJwtService.VerifyAll();
        _mockCacheService.VerifyAll();

        // Check
        Assert.Equal("testJwt", response.AccessToken);
        Assert.Equal("RefreshTokenTest", response.RefreshToken);
    }
}