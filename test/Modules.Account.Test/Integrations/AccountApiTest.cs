using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Account.Core.Abstractions;
using Modules.Account.Core.Commands;
using Modules.Account.Core.Models.Data;
using Shared.Test.Fixtures;
using Xunit;

namespace Modules.Account.Test.Integrations;

[Collection("Container")]
public class AccountApiTest : IDisposable
{
    private readonly RegisterAccountCommand _registerAccountCommand = new()
    {
        AuthenticationProvider = AuthenticationProvider.Self,
        Email = "kangdroid@test.com",
        AuthCode = Guid.NewGuid().ToString(),
        Nickname = "KangDroid"
    };

    private readonly WebApplicationFactory<Program> _webApplicationFactory;

    public AccountApiTest(SharedContainerFixtures containerFixtures)
    {
        var dbConnectionPort = containerFixtures.AzureSqlContainer.GetMappedPublicPort(1433);
        var redisConnectionPort = containerFixtures.RedisTestcontainer.GetMappedPublicPort(6379);
        var configurationStore = new Dictionary<string, string>
        {
            ["ConnectionStrings:DatabaseConnection"] =
                $"Data Source=tcp:localhost,{dbConnectionPort};Initial Catalog={Guid.NewGuid().ToString()};User Id=SA;Password=testPassword@;Encrypt=False",
            ["ConnectionStrings:CacheConnection"] = $"localhost:{redisConnectionPort},abortConnect=False",
            ["JwtSecurityKey"] = "adsfasdfasdfasdfasdfasdfafdsdafs"
        };
        var configuration = new ConfigurationBuilder()
                            .AddInMemoryCollection(configurationStore)
                            .Build();

        _webApplicationFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseConfiguration(configuration);
            });
    }

    public void Dispose()
    {
        _webApplicationFactory.Dispose();
    }

    [Fact(DisplayName = "POST /api/account/join: Join should return 204 NoContent")]
    public async Task Is_Join_Returns_204_NoContent_When_Join_Successful()
    {
        // Let
        var request = _registerAccountCommand;

        // Do
        var httpClient = _webApplicationFactory.CreateClient();
        var response = await httpClient.PostAsJsonAsync("/api/account/join", request);

        // Check
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact(DisplayName = "POST /api/account/join: Join should return 409 Conflict when User tries to re-register user again.")]
    public async Task Is_Join_Returns_409_Conflict_When_User_Tries_Multiple_Join()
    {
        // Let
        await Is_Join_Returns_204_NoContent_When_Join_Successful();

        // Do
        var httpClient = _webApplicationFactory.CreateClient();
        var response = await httpClient.PostAsJsonAsync("/api/account/join", _registerAccountCommand);

        // Check
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact(DisplayName = "POST /api/account/login: Login should return 401 Unauthorized when user's credential is wrong.")]
    public async Task Is_Login_Returns_401_When_Credential_Wrong()
    {
        // Let
        var httpClient = _webApplicationFactory.CreateClient();
        var loginRequest = new LoginCommand
        {
            AuthenticationProvider = AuthenticationProvider.Self,
            Email = "kangdroid@test.com",
            AuthCode = "testPassword@"
        };

        // Do
        var response = await httpClient.PostAsJsonAsync("/api/account/login", loginRequest);

        // Check
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName =
        "POST /api/account/login: Login should return 200 OK With AccessToken when user's credential is correct.")]
    public async Task Is_Login_Returns_200_Ok_When_Credential_Correct()
    {
        // Let
        var httpClient = _webApplicationFactory.CreateClient();
        var id = Guid.NewGuid().ToString();
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
        using var scope = _webApplicationFactory.Services.CreateScope();
        var accountDbContext = scope.ServiceProvider.GetRequiredService<IAccountDbContext>();
        accountDbContext.Accounts.Add(account);
        await accountDbContext.SaveChangesAsync(default);
        var loginRequest = new LoginCommand
        {
            AuthenticationProvider = AuthenticationProvider.Self,
            Email = "kangdroid@test.com",
            AuthCode = "testPassword@"
        };

        // Do
        var response = await httpClient.PostAsJsonAsync("/api/account/login", loginRequest);

        // Check
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}