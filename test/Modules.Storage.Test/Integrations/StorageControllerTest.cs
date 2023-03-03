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

    // [Fact(DisplayName =
    //     "POST /api/storage/upload: UploadBlobFileAsync should return 400 BadRequest when parentFolderId is not actual folder ID.")]
    // public async Task Is_UploadBlobFileAsync_Returns_400_BadRequest_When_ParentFolderId_Not_Folder()
    // {
    //     
    // }

    [Fact(DisplayName =
        "POST /api/storage/upload: UploadBlobFileAsync should return 403 Forbidden when parent folder is not user's one.")]
    public async Task Is_UploadBlobFileAsync_Returns_403_Forbidden_When_ParentFolder_Not_Users()
    {
        // Let
        var httpClient = _webApplicationFactory.CreateClient();
        var firstUser = await httpClient.CreateTestUser();
        var secondUser = await httpClient.CreateTestUser();
        var targetAccessToken = firstUser.AccessToken.AccessToken;

        // Do
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", targetAccessToken);
        await using var stream = "test".CreateStream();
        var formContent = new MultipartFormDataContent();
        formContent.Add(new StringContent(secondUser.AccessTokenInformation.RootId), "parentFolderId");
        formContent.Add(new StreamContent(stream), "fileContents", "test.txt");
        var response = await httpClient.PostAsync("/api/storage/upload", formContent);

        // Check
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact(DisplayName =
        "POST /api/storage/upload: UploadBlobFileAsync should return 404 Not Found when parent folder is not found.")]
    public async Task Is_UploadBlobFileAsync_Returns_404_NotFound_When_ParentFolder_Not_Found()
    {
        // Let
        var httpClient = _webApplicationFactory.CreateClient();
        var firstUser = await httpClient.CreateTestUser();
        var targetAccessToken = firstUser.AccessToken.AccessToken;

        // Do
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", targetAccessToken);
        await using var stream = "test".CreateStream();
        var formContent = new MultipartFormDataContent();
        formContent.Add(new StringContent(ObjectId.Empty.ToString()), "parentFolderId");
        formContent.Add(new StreamContent(stream), "fileContents", "test.txt");
        var response = await httpClient.PostAsync("/api/storage/upload", formContent);

        // Check
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(DisplayName = "POST /api/storage/upload: UploadBlobFileAsync should return 200 OK when all requests are valid.")]
    public async Task Is_UploadBlobFileAsync_Return_200_When_All_Request_Valid()
    {
        // Let
        var httpClient = _webApplicationFactory.CreateClient();
        var firstUser = await httpClient.CreateTestUser();
        var targetAccessToken = firstUser.AccessToken.AccessToken;

        // Do
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", targetAccessToken);
        await using var stream = "test".CreateStream();
        var formContent = new MultipartFormDataContent();
        formContent.Add(new StringContent(firstUser.AccessTokenInformation.RootId), "parentFolderId");
        formContent.Add(new StreamContent(stream), "fileContents", "test.txt");
        var response = await httpClient.PostAsync("/api/storage/upload", formContent);

        // Check
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "DELETE /api/storage/blobId: DeleteBlobAsync should return 400 BadRequest when blobId is rootId")]
    public async Task Is_DeleteBlobAsync_Returns_BadRequest_When_BlobId_RootId()
    {
        // Let
        var httpClient = _webApplicationFactory.CreateClient();
        var user = await httpClient.CreateTestUser();
        var targetAccessToken = user.AccessToken.AccessToken;

        // Do
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", targetAccessToken);
        var response = await httpClient.DeleteAsync($"/api/storage/{user.AccessTokenInformation.RootId}");

        // Check
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName =
        "DELETE /api/storage/blobId: DeleteBlobAsync should return 403 Forbidden when target blob is not user's one.")]
    public async Task Is_DeleteBlobAsync_Returns_Forbidden_When_Target_Blob_Not_Users_One()
    {
        // Let
        var httpClient = _webApplicationFactory.CreateClient();
        var firstUser = await httpClient.CreateTestUser();
        var secondUser = await httpClient.CreateTestUser();
        var targetAccessToken = firstUser.AccessToken.AccessToken;

        // Do
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", targetAccessToken);
        var response = await httpClient.DeleteAsync($"/api/storage/{secondUser.AccessTokenInformation.RootId}");

        // Check
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact(DisplayName =
        "DELETE /api/storage/blobId: DeleteBlobAsync should return 404 NotFound when target blob is not found.")]
    public async Task Is_DeleteBlobAsync_Returns_404_Not_Found_When_Blob_Does_Not_Exists()
    {
        // Let
        var httpClient = _webApplicationFactory.CreateClient();
        var firstUser = await httpClient.CreateTestUser();
        var targetAccessToken = firstUser.AccessToken.AccessToken;

        // Do
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", targetAccessToken);
        var response = await httpClient.DeleteAsync($"/api/storage/{ObjectId.GenerateNewId().ToString()}");

        // Check
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(DisplayName =
        "DELETE /api/storage/blobId: DeleteBlobAsync should return 202 Accepted when successfully deleted files.")]
    public async Task Is_DeleteBlobAsync_Returns_Ok_When_Successfully_Deleted_Files()
    {
        // Let
        var httpClient = _webApplicationFactory.CreateClient();
        var firstUser = await httpClient.CreateTestUser();

        // Create First Folder(/test)
        var testFolder = await httpClient.CreateVirtualFolders(firstUser.AccessToken.AccessToken, new CreateBlobFolderRequest
        {
            FolderName = "test",
            ParentFolderId = firstUser.AccessTokenInformation.RootId
        });

        // Create Second Folder(/test/two)
        var testTwoFolder = await httpClient.CreateVirtualFolders(firstUser.AccessToken.AccessToken,
            new CreateBlobFolderRequest
            {
                FolderName = "two",
                ParentFolderId = testFolder.Id
            });

        // Create Third Folder(/test/two/three)
        var testThreeFolder = await httpClient.CreateVirtualFolders(firstUser.AccessToken.AccessToken,
            new CreateBlobFolderRequest
            {
                FolderName = "three",
                ParentFolderId = testTwoFolder.Id
            });

        // Do
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", firstUser.AccessToken.AccessToken);
        var response = await httpClient.DeleteAsync($"/api/storage/{testFolder.Id}");

        // Check
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact(DisplayName =
        "GET /api/storage/blobId/resolve: ResolveBlobPathAsync should return 404 Not found when invalid blobId provided.")]
    public async Task Is_ResolveBlobPathAsync_Returns_404_NotFound_When_Invalid_BlobId_Provided()
    {
        // Let
        var httpClient = _webApplicationFactory.CreateClient();
        var user = await httpClient.CreateTestUser();

        // Do
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.AccessToken.AccessToken);
        var response = await httpClient.GetAsync($"api/storage/{ObjectId.GenerateNewId()}/resolve");

        // Check
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(DisplayName = "GET /api/storage/blobId/resolve: ResolveBlobPathAsync should return 200 Ok with parent information.")]
    public async Task Is_ResolveBlobPathAsync_Returns_Ok_With_Parent_Information()
    {
        // Let
        var httpClient = _webApplicationFactory.CreateClient();
        var user = await httpClient.CreateTestUser();

        // Do
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.AccessToken.AccessToken);
        var response = await httpClient.GetAsync($"api/storage/{user.AccessTokenInformation.RootId}/resolve");

        // Check
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}