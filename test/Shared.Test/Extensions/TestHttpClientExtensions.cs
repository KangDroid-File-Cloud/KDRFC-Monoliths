using System.Net.Http.Headers;
using System.Net.Http.Json;
using Modules.Account.Core.Commands;
using Modules.Account.Core.Models.Responses;
using Modules.Storage.Core.Models.Requests;
using Modules.Storage.Core.Models.Responses;
using Shared.Test.Helpers;

namespace Shared.Test.Extensions;

public static class TestHttpClientExtensions
{
    public async static Task<HttpResponseMessage> CreateAccountAsync(this HttpClient httpClient,
                                                                     RegisterAccountCommand requestBody,
                                                                     bool validateSuccessfulResponse = true)
    {
        var response = await httpClient.PostAsJsonAsync("/api/account/join", requestBody);

        if (validateSuccessfulResponse)
        {
            response.EnsureSuccessStatusCode();
        }

        return response;
    }

    public async static Task<HttpResponseMessage> LoginToAccountAsync(this HttpClient httpClient, LoginCommand loginCommand,
                                                                      bool validateSuccessfulResponse = true)
    {
        var response = await httpClient.PostAsJsonAsync("/api/account/login", loginCommand);

        if (validateSuccessfulResponse)
        {
            response.EnsureSuccessStatusCode();
        }

        return response;
    }

    public async static Task<TestUserInfo> CreateTestUser(this HttpClient httpClient)
    {
        var testUserInfo = new TestUserInfo();

        // 1. Create Account
        await httpClient.CreateAccountAsync(testUserInfo.RegisterAccountCommand);

        // 2. Login to Account
        testUserInfo.AccessToken = (await (await httpClient.LoginToAccountAsync(testUserInfo.LoginCommand))
                                          .Content.ReadFromJsonAsync<AccessTokenResponse>())!;

        return testUserInfo;
    }

    public async static Task<BlobProjection> CreateVirtualFolders(this HttpClient httpClient, string accessToken,
                                                                  CreateBlobFolderRequest blobFolderRequest)
    {
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var httpResponse = await httpClient.PostAsJsonAsync("/api/storage/folders", blobFolderRequest);

        httpResponse.EnsureSuccessStatusCode();
        return await httpResponse.Content.ReadFromJsonAsync<BlobProjection>();
    }

    public async static Task<BlobProjection> UploadFileAsync(this HttpClient httpClient, string accessToken, string fileName,
                                                             string parentId, Stream stream)
    {
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var formContent = new MultipartFormDataContent();
        formContent.Add(new StringContent(parentId), "parentFolderId");
        formContent.Add(new StreamContent(stream), "fileContents", fileName);
        var uploadResponse = await httpClient.PostAsync("/api/storage/upload", formContent);

        return await uploadResponse.Content.ReadFromJsonAsync<BlobProjection>();
    }
}