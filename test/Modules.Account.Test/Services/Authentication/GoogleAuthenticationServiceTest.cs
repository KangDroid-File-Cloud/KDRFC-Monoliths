using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Modules.Account.Core.Abstractions;
using Modules.Account.Core.Commands;
using Modules.Account.Core.Models.Data;
using Modules.Account.Core.Models.Responses;
using Modules.Account.Core.Services.Authentication;
using Modules.Account.Infrastructure.Persistence;
using Moq;
using Moq.Contrib.HttpClient;
using Shared.Core.Exceptions;
using Shared.Core.Services;
using Xunit;

namespace Modules.Account.Test.Services.Authentication;

public class GoogleAuthenticationServiceTest
{
    private readonly IAccountDbContext _accountDbContext;
    private readonly IConfiguration _configuration;
    private readonly GoogleAuthenticationService _googleAuthenticationService;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly Mock<IJwtService> _mockJwtService;

    public GoogleAuthenticationServiceTest()
    {
        _accountDbContext = new AccountDbContext(new DbContextOptionsBuilder<AccountDbContext>()
                                                 .UseInMemoryDatabase(Ulid.NewUlid().ToString()).Options);
        _configuration = new ConfigurationBuilder()
                         .AddInMemoryCollection(new Dictionary<string, string>
                         {
                             ["OAuth:Google:ClientId"] = "testClientId",
                             ["OAuth:Google:ClientSecret"] = "testClientSecret"
                         })
                         .Build();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _mockJwtService = new Mock<IJwtService>();

        _googleAuthenticationService = new GoogleAuthenticationService(_accountDbContext, _mockJwtService.Object,
            _mockHttpClientFactory.Object, NullLogger<GoogleAuthenticationService>.Instance, _configuration);
    }

    [Fact(DisplayName =
        "AuthenticateAsync: AuthenticateAsync should throw an ApiException with InternalServerError when getting OAuth Access Token fails.")]
    public async Task Is_AuthenticateAsync_Throws_ApiException_When_Getting_OAuthAccessToken_Fails()
    {
        // Let
        _mockHttpMessageHandler.SetupRequest(HttpMethod.Post, "https://oauth2.googleapis.com/token")
                               .ReturnsResponse(HttpStatusCode.BadRequest);
        _mockHttpClientFactory.Setup(a => a.CreateClient(Options.DefaultName))
                              .Returns(_mockHttpMessageHandler.CreateClient());
        var loginCommand = new LoginCommand
        {
            AuthenticationProvider = AuthenticationProvider.Google,
            Email = "kangdroid@test.com",
            AuthCode = Ulid.NewUlid().ToString()
        };

        // Do
        var exception =
            await Assert.ThrowsAnyAsync<ApiException>(() => _googleAuthenticationService.AuthenticateAsync(loginCommand));

        // Verify
        _mockHttpMessageHandler.VerifyAll();
        _mockHttpClientFactory.VerifyAll();

        // Check
        Assert.Equal(StatusCodes.Status500InternalServerError, exception.StatusCode);
    }

    [Fact(DisplayName =
        "AuthenticateAsync: AuthenticateAsync should throw an ApiException with InternalServerError when getting OAuthUserInfo fails.")]
    public async Task Is_AuthenticateAsync_Throws_ApiException_With_InternalServerError_When_Getting_UserInfo_Fails()
    {
        // Let
        _mockHttpMessageHandler.SetupRequest(HttpMethod.Post, "https://oauth2.googleapis.com/token")
                               .ReturnsJsonResponse(new GoogleAccessTokenResponse
                               {
                                   AccessToken = Ulid.NewUlid().ToString()
                               });
        _mockHttpMessageHandler.SetupRequest(HttpMethod.Get, "https://www.googleapis.com/oauth2/v2/userinfo")
                               .ReturnsResponse(HttpStatusCode.BadRequest);
        _mockHttpClientFactory.Setup(a => a.CreateClient(Options.DefaultName))
                              .Returns(_mockHttpMessageHandler.CreateClient());
        var loginCommand = new LoginCommand
        {
            AuthenticationProvider = AuthenticationProvider.Google,
            Email = "kangdroid@test.com",
            AuthCode = Ulid.NewUlid().ToString()
        };

        // Do
        var exception =
            await Assert.ThrowsAnyAsync<ApiException>(() => _googleAuthenticationService.AuthenticateAsync(loginCommand));

        // Verify
        _mockHttpMessageHandler.VerifyAll();
        _mockHttpClientFactory.VerifyAll();

        // Check
        Assert.Equal(StatusCodes.Status500InternalServerError, exception.StatusCode);
    }

    [Fact(DisplayName =
        "AuthenticateAsync: AuthenticateAsync should throw an ApiException with 404 Not Found when there is no user registered within OAuth ID.")]
    public async Task Is_AuthenticateAsync_Throws_ApiException_With_Not_Found_When_No_User_Registered()
    {
        // Let
        _mockHttpMessageHandler.SetupRequest(HttpMethod.Post, "https://oauth2.googleapis.com/token")
                               .ReturnsJsonResponse(new GoogleAccessTokenResponse
                               {
                                   AccessToken = Ulid.NewUlid().ToString()
                               });
        _mockHttpMessageHandler.SetupRequest(HttpMethod.Get, "https://www.googleapis.com/oauth2/v2/userinfo")
                               .ReturnsJsonResponse(new GoogleMeResponse
                               {
                                   Id = Ulid.NewUlid().ToString(),
                                   Email = "kangdroid@test.com"
                               });
        _mockHttpClientFactory.Setup(a => a.CreateClient(Options.DefaultName))
                              .Returns(_mockHttpMessageHandler.CreateClient());
        _mockJwtService.Setup(a => a.GenerateJwt(It.IsAny<List<Claim>>(), It.IsAny<DateTime>()))
                       .Returns("joinToken");
        var loginCommand = new LoginCommand
        {
            AuthenticationProvider = AuthenticationProvider.Google,
            AuthCode = Ulid.NewUlid().ToString()
        };

        // Do
        var exception =
            await Assert.ThrowsAnyAsync<ApiException>(() => _googleAuthenticationService.AuthenticateAsync(loginCommand));

        // Verify
        _mockHttpMessageHandler.VerifyAll();
        _mockHttpClientFactory.VerifyAll();
        _mockJwtService.VerifyAll();

        // Check
        Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
        Assert.True(exception.CustomJsonBody is JoinTokenResponse);
        Assert.Equal("joinToken", ((JoinTokenResponse)exception.CustomJsonBody).JoinToken);
    }

    [Fact(DisplayName = "AuthenticateAsync: AuthenticateAsync should return credential of user if all OAuth flows gone OK.")]
    public async Task Is_AuthenticateAsync_Returns_Credential_When_All_Flows_Gone_Ok()
    {
        // Let
        var oAuthUserId = Ulid.NewUlid().ToString();
        var userId = Ulid.NewUlid().ToString();
        _mockHttpMessageHandler.SetupRequest(HttpMethod.Post, "https://oauth2.googleapis.com/token")
                               .ReturnsJsonResponse(new GoogleAccessTokenResponse
                               {
                                   AccessToken = Ulid.NewUlid().ToString()
                               });
        _mockHttpMessageHandler.SetupRequest(HttpMethod.Get, "https://www.googleapis.com/oauth2/v2/userinfo")
                               .ReturnsJsonResponse(new GoogleMeResponse
                               {
                                   Id = oAuthUserId,
                                   Email = "kangdroid@test.com"
                               });
        _mockHttpClientFactory.Setup(a => a.CreateClient(Options.DefaultName))
                              .Returns(_mockHttpMessageHandler.CreateClient());
        _accountDbContext.Credentials.Add(new Credential
        {
            AuthenticationProvider = AuthenticationProvider.Google,
            ProviderId = oAuthUserId,
            Account = new Core.Models.Data.Account
            {
                Id = userId,
                Email = "kangdroid@test.com",
                NickName = "KangDroid"
            },
            UserId = userId
        });
        await _accountDbContext.SaveChangesAsync(default);
        var loginCommand = new LoginCommand
        {
            AuthenticationProvider = AuthenticationProvider.Google,
            AuthCode = Ulid.NewUlid().ToString()
        };

        // Do
        var response = await _googleAuthenticationService.AuthenticateAsync(loginCommand);

        // Verify
        _mockHttpMessageHandler.VerifyAll();
        _mockHttpClientFactory.VerifyAll();

        // Check
        Assert.Equal(AuthenticationProvider.Google, response.AuthenticationProvider);
        Assert.Equal(oAuthUserId, response.ProviderId);
    }
}