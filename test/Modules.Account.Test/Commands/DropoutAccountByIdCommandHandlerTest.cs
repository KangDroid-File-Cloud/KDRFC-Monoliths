using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Modules.Account.Core.Abstractions;
using Modules.Account.Core.Commands;
using Modules.Account.Core.Models.Data;
using Modules.Account.Infrastructure.Persistence;
using Shared.Core.Exceptions;
using Xunit;

namespace Modules.Account.Test.Commands;

public class DropoutAccountByIdCommandHandlerTest
{
    private readonly IAccountDbContext _accountDbContext;
    private readonly DropoutAccountByIdCommandHandler _commandHandler;

    public DropoutAccountByIdCommandHandlerTest()
    {
        var option = new DbContextOptionsBuilder<AccountDbContext>()
                     .UseInMemoryDatabase(Ulid.NewUlid().ToString())
                     .Options;
        _accountDbContext = new AccountDbContext(option);
        _commandHandler = new DropoutAccountByIdCommandHandler(_accountDbContext);
    }

    private async Task<Core.Models.Data.Account> CreateAccountAsync()
    {
        var id = Ulid.NewUlid().ToString();
        var account = new Core.Models.Data.Account
        {
            Id = id,
            Email = "kangdroid@test.com",
            NickName = "KangDroid",
            Credentials = new List<Credential>
            {
                new()
                {
                    UserId = id,
                    AuthenticationProvider = AuthenticationProvider.Self,
                    ProviderId = "kangdroid@test.com",
                    Key = "testPassword@"
                }
            }
        };

        _accountDbContext.Accounts.Add(account);
        await _accountDbContext.SaveChangesAsync(default);

        return account;
    }

    [Fact(DisplayName =
        "Handle: Handle should throw ApiException with HttpStatusCode NotFound when cannot find user with given command.")]
    public async Task Is_Handle_Throws_ApiException_With_404_When_User_Not_Found()
    {
        // Let
        var request = new DropoutUserByIdCommand
        {
            UserId = Ulid.NewUlid().ToString()
        };

        // Do
        var exception = await Assert.ThrowsAnyAsync<ApiException>(() => _commandHandler.Handle(request, default));

        // Check
        Assert.NotNull(exception);
        Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
    }

    [Fact(DisplayName = "Handle: Handle should removes credential and set user deleted flags to true when succeed.")]
    public async Task Is_Handle_Removes_Credential_And_Set_Deleted_Flags()
    {
        // Let
        var account = await CreateAccountAsync();
        var request = new DropoutUserByIdCommand
        {
            UserId = account.Id
        };

        // Do
        await _commandHandler.Handle(request, default);

        // Check Account is still in DB
        var dataList = await _accountDbContext.Accounts
                                              .IgnoreQueryFilters()
                                              .Include(a => a.Credentials)
                                              .ToListAsync();
        Assert.Single(dataList);

        // Check Account data itself.
        var accountData = dataList.First();
        Assert.Equal(account.Id, accountData.Id);
        Assert.Equal(account.Email, accountData.Email);
        Assert.Equal(account.NickName, accountData.NickName);
        Assert.True(account.IsDeleted);

        // Check credential is empty.
        Assert.Empty(accountData.Credentials);
        Assert.Empty(await _accountDbContext.Credentials.ToListAsync());
    }
}