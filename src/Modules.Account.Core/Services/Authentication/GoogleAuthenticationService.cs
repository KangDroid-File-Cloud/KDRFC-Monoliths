using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Modules.Account.Core.Abstractions;
using Modules.Account.Core.Models.Internal;
using Newtonsoft.Json;
using Shared.Core.Exceptions;
using Shared.Core.Services;

namespace Modules.Account.Core.Services.Authentication;

public class GoogleAccessTokenResponse
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }
}

public class GoogleMeResponse
{
    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }
}

[ExcludeFromCodeCoverage]
public class GoogleAuthenticationService : OAuthServiceBase
{
    private readonly string _googleClientId;
    private readonly string _googleClientSecret;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GoogleAuthenticationService> _logger;

    public GoogleAuthenticationService(IAccountDbContext accountDbContext,
                                       IJwtService jwtService, IHttpClientFactory httpClientFactory,
                                       ILogger<GoogleAuthenticationService> logger, IConfiguration configuration) : base(
        accountDbContext, jwtService)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _googleClientId = configuration["OAuth:Google:ClientId"];
        _googleClientSecret = configuration["OAuth:Google:ClientSecret"];
    }

    protected async override Task<string> GetOAuthAccessTokenAsync(string authCode)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["redirect_uri"] = "postmessage",
                ["client_id"] = _googleClientId,
                ["client_secret"] = _googleClientSecret,
                ["code"] = authCode
            }));

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("[Google] Failed to get access token from provider: {responseBody}",
                await response.Content.ReadAsStringAsync());
            throw new ApiException(HttpStatusCode.InternalServerError,
                "[Google] Failed to get OAuth Access Token from OAuth Provider.");
        }

        var accessTokenResponse =
            JsonConvert.DeserializeObject<GoogleAccessTokenResponse>(await response.Content.ReadAsStringAsync())!;
        return accessTokenResponse.AccessToken;
    }

    protected async override Task<OAuthLoginResult> GetOAuthUserInfoAsync(string accessToken)
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");

        // When Google's response is not successful, return null
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("[Google]: Failed to get OAuth User Information: {responseBody}",
                await response.Content.ReadAsStringAsync());
            throw new ApiException(HttpStatusCode.InternalServerError,
                "[Google] Failed to get OAuth User information from OAuth Provider.");
        }

        // Get Response
        var googleMeResponse = JsonConvert.DeserializeObject<GoogleMeResponse>(await response.Content.ReadAsStringAsync())!;

        return new OAuthLoginResult
        {
            Id = googleMeResponse.Id,
            Email = googleMeResponse.Email
        };
    }
}