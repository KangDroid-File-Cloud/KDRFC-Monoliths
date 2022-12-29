using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Modules.Account.Core.Commands;
using Modules.Account.Core.Models.Data;
using Modules.Account.Core.Models.Responses;
using Shared.Core.Services;
using Shared.Test.Extensions;
using Shared.Test.Fixtures;
using Xunit;

namespace Modules.Storage.Test.Integrations;

[Collection("Container")]
public class StorageControllerTest : IDisposable
{
    private readonly RegisterAccountCommand _registerAccountCommand = new()
    {
        AuthenticationProvider = AuthenticationProvider.Self,
        Email = "kangdroid@test.com",
        AuthCode = Ulid.NewUlid().ToString(),
        Nickname = "KangDroid"
    };

    private readonly WebApplicationFactory<Program> _webApplicationFactory;

    public StorageControllerTest(SharedContainerFixtures containerFixtures)
    {
        var dbConnectionPort = containerFixtures.AzureSqlContainer.GetMappedPublicPort(1433);
        var redisConnectionPort = containerFixtures.RedisTestcontainer.GetMappedPublicPort(6379);
        var configurationStore = new Dictionary<string, string>
        {
            ["ConnectionStrings:MongoDbConnection"] =
                $"mongodb://{containerFixtures.MongoDbContainerConfiguration.Username}:{containerFixtures.MongoDbContainerConfiguration.Password}@localhost:{containerFixtures.MongoDbContainerConfiguration.Port}",
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

    private LoginCommand CreateLoginCommandFromRegister(RegisterAccountCommand command)
    {
        return new LoginCommand
        {
            AuthenticationProvider = command.AuthenticationProvider,
            Email = command.Email,
            AuthCode = command.AuthCode
        };
    }

    [Fact(DisplayName = "GET /api/storage: ListFolderAsync should return list of files when requested.")]
    public async Task Is_ListFolderAsync_Returns_List_Of_Files_When_Executed()
    {
        // Let
        var httpClient = _webApplicationFactory.CreateClient();
        var registerCommand = _registerAccountCommand;
        var loginCommand = CreateLoginCommandFromRegister(registerCommand);
        await httpClient.CreateAccountAsync(registerCommand);
        var accessToken = (await (await httpClient.LoginToAccountAsync(loginCommand))
                                 .Content.ReadFromJsonAsync<AccessTokenResponse>())!.AccessToken;
        var userInformation = new
        {
            AccountId = new JwtSecurityToken(accessToken).Claims.First(a => a.Type == JwtRegisteredClaimNames.Sub).Value,
            RootId = new JwtSecurityToken(accessToken).Claims.First(a => a.Type == KDRFCCommonClaimName.RootId).Value
        };

        // Do
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.GetAsync($"/api/storage/list?folderId={userInformation.RootId}");

        // Check
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}