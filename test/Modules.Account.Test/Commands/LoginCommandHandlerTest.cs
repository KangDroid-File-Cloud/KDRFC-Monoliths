using System.Security.Claims;
using MediatR;
using Modules.Account.Core.Commands;
using Modules.Account.Core.Commands.Handlers;
using Modules.Account.Core.Models.Data;
using Modules.Account.Core.Services.Authentication;
using Moq;
using Shared.Core.Abstractions;
using Shared.Core.Commands;
using Shared.Core.Services;
using Xunit;

namespace Modules.Account.Test.Commands;

public class LoginCommandHandlerTest
{
    private readonly AuthenticationProviderFactory _authenticationProviderFactory;
    private readonly LoginAccountCommandHandler _commandHandler;

    // Authentication Providers
    private readonly Mock<IAuthenticationService> _mockAuthenticationService;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<IMediator> _mockMediator;

    public LoginCommandHandlerTest()
    {
        _mockAuthenticationService = new Mock<IAuthenticationService>();
        _mockCacheService = new Mock<ICacheService>();
        _mockJwtService = new Mock<IJwtService>();
        _mockMediator = new Mock<IMediator>();
        _authenticationProviderFactory = provider => _mockAuthenticationService.Object;

        _commandHandler = new LoginAccountCommandHandler(_authenticationProviderFactory, _mockMediator.Object,
            _mockCacheService.Object, _mockJwtService.Object);
    }

    [Fact(DisplayName = "Handle: Handle should return AccessTokenResponse when provider returned correct credential.")]
    public async Task Is_Handle_Returns_AccessTokenResponse_When_Provider_Returns_Correct_Credential()
    {
        // Let
        var loginCommand = new LoginCommand
        {
            AuthenticationProvider = AuthenticationProvider.Self,
            Email = "kangdroid@test.com",
            AuthCode = "testPassword@"
        };
        var userId = Ulid.NewUlid().ToString();
        _mockAuthenticationService.Setup(a => a.AuthenticateAsync(loginCommand))
                                  .ReturnsAsync(new Credential
                                  {
                                      UserId = userId,
                                      Account = new Core.Models.Data.Account
                                      {
                                          Email = loginCommand.Email,
                                          Id = userId,
                                          IsDeleted = false,
                                          NickName = "KangDroid"
                                      },
                                      AuthenticationProvider = AuthenticationProvider.Self,
                                      Key = "testPassword@",
                                      ProviderId = loginCommand.Email
                                  });
        _mockMediator.Setup(a => a.Send(It.IsAny<GetRootByAccountIdCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync("root");
        _mockJwtService.Setup(a => a.GenerateJwt(It.IsAny<List<Claim>>(), null))
                       .Returns("accessToken");
        _mockCacheService
            .Setup(a => a.GetItemOrCreateAsync(It.IsAny<string>(), It.IsAny<Func<Task<RefreshToken>>>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync(new RefreshToken
            {
                Token = "refreshToken"
            });
        _mockCacheService.Setup(a =>
            a.SetItemAsync(AccountCacheKeys.RefreshTokenKey(userId), It.IsAny<object>(), It.IsAny<TimeSpan>()));

        // Do
        var response = await _commandHandler.Handle(loginCommand, default);

        // Verify
        _mockMediator.VerifyAll();
        _mockJwtService.VerifyAll();
        _mockJwtService.VerifyAll();
        _mockCacheService.VerifyAll();

        // Check
        Assert.Equal("accessToken", response.AccessToken);
        Assert.Equal("refreshToken", response.RefreshToken);
    }
}