using System.Net.Http.Json;
using Modules.Account.Core.Commands;
using Modules.Account.Core.Models.Data;
using Modules.Account.Core.Models.Responses;
using Shared.Test.Helpers;

namespace Shared.Test.Extensions;

public static class TestHttpClientExtensions
{
    public async static Task<HttpResponseMessage> CreateAccountAsync(this HttpClient httpClient,
                                                                     RegisterAccountCommand requestBody,
                                                                     bool validateSuccessfulResponse = true)
    {
        var response = await httpClient.PostAsJsonAsync("/api/account/join", requestBody);

        if (validateSuccessfulResponse) response.EnsureSuccessStatusCode();
        return response;
    }

    public async static Task<HttpResponseMessage> LoginToAccountAsync(this HttpClient httpClient, LoginCommand loginCommand,
                                                                      bool validateSuccessfulResponse = true)
    {
        var response = await httpClient.PostAsJsonAsync("/api/account/login", loginCommand);

        if (validateSuccessfulResponse) response.EnsureSuccessStatusCode();
        return response;
    }

    public async static Task<TestUserInfo> CreateTestUser(this HttpClient httpClient)
    {
        var testUserInfo = new TestUserInfo
        {
            RegisterAccountCommand = new RegisterAccountCommand
            {
                Email = $"{Ulid.NewUlid().ToString()}@test.com",
                Nickname = "TestNickName",
                AuthenticationProvider = AuthenticationProvider.Self,
                AuthCode = "testPassword@"
            }
        };

        // 1. Create Account
        await httpClient.CreateAccountAsync(testUserInfo.RegisterAccountCommand);

        // 2. Login to Account
        testUserInfo.AccessToken = (await (await httpClient.LoginToAccountAsync(testUserInfo.LoginCommand))
                                          .Content.ReadFromJsonAsync<AccessTokenResponse>())!;

        return testUserInfo;
    }
}