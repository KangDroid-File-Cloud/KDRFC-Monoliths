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

public class KakaoAccessTokenResponse
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }
}

public class KakaoMeResponse
{
    public string Id { get; set; }

    [JsonProperty("kakao_account")]
    public KakaoAccount KakaoAccount { get; set; }
}

public class KakaoAccount
{
    public string? Email { get; set; }
}

[ExcludeFromCodeCoverage]
public class KakaoAuthenticationService : OAuthServiceBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _kakaoClientId;
    private readonly string _kakaoClientSecret;
    private readonly ILogger<KakaoAuthenticationService> _logger;

    public KakaoAuthenticationService(IAccountDbContext accountDbContext,
                                      IJwtService jwtService, IHttpClientFactory httpClientFactory,
                                      ILogger<KakaoAuthenticationService> logger, IConfiguration configuration) : base(
        accountDbContext, jwtService)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _kakaoClientId = configuration["OAuth:Kakao:ClientId"];
        _kakaoClientSecret = configuration["OAuth:Kakao:ClientSecret"];
    }

    protected async override Task<string> GetOAuthAccessTokenAsync(string authCode, string requestOrigin)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.PostAsync("https://kauth.kakao.com/oauth/token", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["redirect_uri"] = $"{requestOrigin}/auth/redirect/kakao",
                ["client_id"] = _kakaoClientId,
                ["client_secret"] = _kakaoClientSecret,
                ["code"] = authCode
            }));

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("[Kakao] Failed to get access token from provider: {responseBody}",
                await response.Content.ReadAsStringAsync());
            throw new ApiException(HttpStatusCode.InternalServerError,
                "[Kakao] Failed to get OAuth Access Token from OAuth Provider.");
        }

        var accessTokenResponse =
            JsonConvert.DeserializeObject<KakaoAccessTokenResponse>(await response.Content.ReadAsStringAsync())!;
        return accessTokenResponse.AccessToken;
    }

    protected async override Task<OAuthLoginResult> GetOAuthUserInfoAsync(string accessToken)
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await httpClient.GetAsync("https://kapi.kakao.com/v2/user/me");

        // When Google's response is not successful, return null
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("[Kakao]: Failed to get OAuth User Information: {responseBody}",
                await response.Content.ReadAsStringAsync());
            throw new ApiException(HttpStatusCode.InternalServerError,
                "[Kakao] Failed to get OAuth User information from OAuth Provider.");
        }

        // Get Response
        var kakaoMeResponse = JsonConvert.DeserializeObject<KakaoMeResponse>(await response.Content.ReadAsStringAsync())!;

        return new OAuthLoginResult
        {
            Id = kakaoMeResponse.Id,
            Email = kakaoMeResponse.KakaoAccount.Email ?? throw new ApiException(HttpStatusCode.InternalServerError,
                "Cannot get account email from kakao response!")
        };
    }
}