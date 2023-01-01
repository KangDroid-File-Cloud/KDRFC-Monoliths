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
using Modules.Storage.Core.Models.Requests;
using MongoDB.Bson;
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

    [Fact(DisplayName =
        "POST /api/storage/folders: CreateFolderAsync should return 404 Not Found when parent folder is not found.")]
    public async Task Is_CreateFolderAsync_Returns_NotFound_When_ParentFolder_Not_Found()
    {
        // Let
        var httpClient = _webApplicationFactory.CreateClient();
        var registerCommand = _registerAccountCommand;
        var loginCommand = CreateLoginCommandFromRegister(registerCommand);
        await httpClient.CreateAccountAsync(registerCommand);
        var accessToken = (await (await httpClient.LoginToAccountAsync(loginCommand))
                                 .Content.ReadFromJsonAsync<AccessTokenResponse>())!.AccessToken;
        var request = new CreateBlobFolderRequest
        {
            ParentFolderId = ObjectId.Empty.ToString(),
            FolderName = "KangDroidTest"
        };

        // Do
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.PostAsJsonAsync("/api/storage/folders", request);

        // Check
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(DisplayName = "POST /api/storage/folders: CreateFolderAsync should return blob(folder) information well.")]
    public async Task Is_CreateFolderAsync_Returns_Blob_Information_When_Requested()
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
        var request = new CreateBlobFolderRequest
        {
            ParentFolderId = userInformation.RootId,
            FolderName = "KangDroidTest"
        };

        // Do
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.PostAsJsonAsync("/api/storage/folders", request);

        // Check
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}