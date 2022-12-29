using System.Net.Http.Json;
using Modules.Account.Core.Commands;

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
}