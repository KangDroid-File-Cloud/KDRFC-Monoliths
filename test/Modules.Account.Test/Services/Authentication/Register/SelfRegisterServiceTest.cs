using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Modules.Account.Core.Abstractions;
using Modules.Account.Core.Commands;
using Modules.Account.Core.Models.Data;
using Modules.Account.Core.Services.Register;
using Modules.Account.Infrastructure.Persistence;
using Shared.Core.Exceptions;
using Xunit;

namespace Modules.Account.Test.Services.Authentication.Register;

public class SelfRegisterServiceTest
{
    private readonly IAccountDbContext _accountDbContext;
    private readonly SelfRegisterService _selfRegisterService;

    public SelfRegisterServiceTest()
    {
        _accountDbContext = new AccountDbContext(new DbContextOptionsBuilder<AccountDbContext>()
                                                 .UseInMemoryDatabase(Ulid.NewUlid().ToString())
                                                 .Options);
        _selfRegisterService = new SelfRegisterService(_accountDbContext);
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
        await _selfRegisterService.CreateAccountAsync(request);

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

        var request = new RegisterAccountCommand
        {
            AuthenticationProvider = AuthenticationProvider.Self,
            Email = "kangdroid@test.com",
            Nickname = "kangdroid",
            AuthCode = "testPassword@"
        };

        // Do
        var exception =
            await Assert.ThrowsAnyAsync<ApiException>(() => _selfRegisterService.CreateAccountAsync(request));

        // Check Exception's Value
        Assert.Equal(StatusCodes.Status409Conflict, exception.StatusCode);
    }
}