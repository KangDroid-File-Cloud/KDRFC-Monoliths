using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Modules.Account.Core.Commands;
using Modules.Account.Core.Models.Data;
using Shared.Test.Extensions;
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
        AuthCode = Ulid.NewUlid().ToString(),
        Nickname = "KangDroid"
    };

    private readonly WebApplicationFactory<Program> _webApplicationFactory;

    public AccountApiTest(SharedContainerFixtures containerFixtures)
    {
        var dbConnectionPort = containerFixtures.AzureSqlContainer.GetMappedPublicPort(1433);
        var redisConnectionPort = containerFixtures.RedisTestcontainer.GetMappedPublicPort(6379);
        var mongoConnectionPort = containerFixtures.MongoDbTestContainer.GetMappedPublicPort(27017);
        var configurationStore = new Dictionary<string, string>
        {
            ["ConnectionStrings:MongoDbConnection"] =
                $"mongodb://{containerFixtures.MongoDbContainerConfiguration.Username}:{containerFixtures.MongoDbContainerConfiguration.Password}@localhost:{mongoConnectionPort}",
            ["ConnectionStrings:DatabaseConnection"] =
                $"Data Source=tcp:localhost,{dbConnectionPort};Initial Catalog={Ulid.NewUlid().ToString()};User Id=SA;Password=testPassword@;Encrypt=False",
            ["ConnectionStrings:CacheConnection"] = $"localhost:{redisConnectionPort},abortConnect=False",
            ["JwtSecurityKey"] = "adsfasdfasdfasdfasdfasdfafdsdafs",
            ["MongoDb:DatabaseName"] = Ulid.NewUlid().ToString(),
            ["EnableConsoleMetricsExporter"] = "false"
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
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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
        var registerInfo = _registerAccountCommand;
        await httpClient.CreateAccountAsync(registerInfo);
        var loginRequest = new LoginCommand
        {
            AuthenticationProvider = AuthenticationProvider.Self,
            Email = registerInfo.Email,
            AuthCode = registerInfo.AuthCode
        };

        // Do
        var response = await httpClient.PostAsJsonAsync("/api/account/login", loginRequest);

        // Check
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "DELETE /api/account/dropout: Dropout should return 401 when access token does not exists.")]
    public async Task Is_Delete_Returns_401_When_AccessToken_Not_Exists()
    {
        // Let: N/A
        var httpClient = _webApplicationFactory.CreateClient();

        // Do
        var response = await httpClient.DeleteAsync("api/account/dropout");

        // Check
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "DELETE /api/account/dropout: Dropout should return 401 when access token malformed.")]
    public async Task Is_Delete_Returns_401_When_AccessToken_Malformed()
    {
        // Let: N/A
        var httpClient = _webApplicationFactory.CreateClient();

        // Do
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "asdffdsasdf");
        var response = await httpClient.DeleteAsync("api/account/dropout");

        // Check
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName =
        "DELETE /api/account/dropout: Dropout should return 204 NoContent when account dropout successfully handled.")]
    public async Task Is_Delete_Returns_204_Ok_When_Successfully_Handled()
    {
        // Let
        var httpClient = _webApplicationFactory.CreateClient();
        var testUser = await httpClient.CreateTestUser();
        var accessToken = testUser.AccessToken.AccessToken;

        // Do
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.DeleteAsync("api/account/dropout");

        // Check
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}