using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Modules.Storage.Core.Models.Requests;
using MongoDB.Bson;
using Shared.Test.Extensions;
using Shared.Test.Fixtures;
using Xunit;

namespace Modules.Storage.Test.Integrations;

[Collection("Container")]
public class StorageControllerTest : IDisposable
{
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

    [Fact(DisplayName = "GET /api/storage: ListFolderAsync should return list of files when requested.")]
    public async Task Is_ListFolderAsync_Returns_List_Of_Files_When_Executed()
    {
        // Let
        var httpClient = _webApplicationFactory.CreateClient();
        var testUser = await httpClient.CreateTestUser();
        var accessToken = testUser.AccessToken.AccessToken;

        // Do
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.GetAsync($"/api/storage/list?folderId={testUser.AccessTokenInformation.RootId}");

        // Check
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName =
        "POST /api/storage/folders: CreateFolderAsync should return 403 Forbidden when parent folder is not user's one.")]
    public async Task Is_CreateFolderAsync_Returns_Forbidden_When_Parent_Folder_Not_Users()
    {
        // Let
        var httpClient = _webApplicationFactory.CreateClient();
        var firstTestUser = await httpClient.CreateTestUser();
        var secondTestUser = await httpClient.CreateTestUser();
        var request = new CreateBlobFolderRequest
        {
            ParentFolderId = secondTestUser.AccessTokenInformation.RootId,
            FolderName = "KangDroidTest"
        };

        // Do
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", firstTestUser.AccessToken.AccessToken);
        var response = await httpClient.PostAsJsonAsync("/api/storage/folders", request);

        // Check
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact(DisplayName =
        "POST /api/storage/folders: CreateFolderAsync should return 404 Not Found when parent folder is not found.")]
    public async Task Is_CreateFolderAsync_Returns_NotFound_When_ParentFolder_Not_Found()
    {
        // Let
        var httpClient = _webApplicationFactory.CreateClient();
        var testUser = await httpClient.CreateTestUser();
        var accessToken = testUser.AccessToken.AccessToken;

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
        var testUser = await httpClient.CreateTestUser();
        var accessToken = testUser.AccessToken.AccessToken;
        var request = new CreateBlobFolderRequest
        {
            ParentFolderId = testUser.AccessTokenInformation.RootId,
            FolderName = "KangDroidTest"
        };

        // Do
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.PostAsJsonAsync("/api/storage/folders", request);

        // Check
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName =
        "GET /api/storage/blobId: GetBlobDetailsAsync should return 403 Forbidden when blob is not user's one.")]
    public async Task Is_GetBlobDetailsAsync_Returns_403_Forbidden_When_Blob_Is_Not_User_One()
    {
        // Let
        var httpClient = _webApplicationFactory.CreateClient();
        var firstUser = await httpClient.CreateTestUser();
        var secondUser = await httpClient.CreateTestUser();
        var targetAccessToken = firstUser.AccessToken.AccessToken;

        // Do
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", targetAccessToken);
        var response = await httpClient.GetAsync($"/api/storage/{secondUser.AccessTokenInformation.RootId}");

        // Check
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact(DisplayName = "GET /api/storage/blobId: GetBlobDetailsAsync should return 404 NotFound when blob is not found.")]
    public async Task Is_GetBlobDetailsAsync_Returns_404_NotFound_When_Blob_Is_Not_Found()
    {
        // Let
        var httpClient = _webApplicationFactory.CreateClient();
        var firstUser = await httpClient.CreateTestUser();
        var targetAccessToken = firstUser.AccessToken.AccessToken;

        // Do
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", targetAccessToken);
        var response = await httpClient.GetAsync($"/api/storage/{ObjectId.Empty.ToString()}");

        // Check
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(DisplayName = "GET /api/storage/blobId: GetBlobDetailsAsync should return 200 OK when all requests are valid.")]
    public async Task Is_GetBlobDetailsAsync_Returns_200_Ok_When_All_Request_Are_Valid()
    {
        // Let
        var httpClient = _webApplicationFactory.CreateClient();
        var firstUser = await httpClient.CreateTestUser();
        var targetAccessToken = firstUser.AccessToken.AccessToken;

        // Do
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", targetAccessToken);
        var response = await httpClient.GetAsync($"/api/storage/{firstUser.AccessTokenInformation.RootId}");

        // Check
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}