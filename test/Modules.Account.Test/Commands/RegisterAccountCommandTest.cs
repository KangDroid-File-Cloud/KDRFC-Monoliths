using System.Security.Claims;
using MediatR;
using Modules.Account.Core.Commands;
using Modules.Account.Core.Commands.Handlers;
using Modules.Account.Core.Models.Data;
using Modules.Account.Core.Services.Register;
using Moq;
using Shared.Core.Abstractions;
using Shared.Core.Commands;
using Shared.Core.Services;
using Xunit;

namespace Modules.Account.Test.Commands;

public class RegisterAccountCommandTest
{
    private readonly RegisterAccountCommandHandler _commandHandler;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<IJwtService> _mockJwtService;

    // IMediator
    private readonly Mock<IMediator> _mockMediator;

    // Authentication Providers
    private readonly Mock<IRegisterService> _mockRegisterService;
    private readonly RegisterProviderFactory _registerProviderFactory;

    public RegisterAccountCommandTest()
    {
        _mockRegisterService = new Mock<IRegisterService>();
        _registerProviderFactory = _ => _mockRegisterService.Object;
        _mockMediator = new Mock<IMediator>();
        _mockJwtService = new Mock<IJwtService>();
        _mockCacheService = new Mock<ICacheService>();
        _commandHandler = new RegisterAccountCommandHandler(_mockMediator.Object, _registerProviderFactory,
            _mockJwtService.Object, _mockCacheService.Object);
    }

    [Fact(DisplayName = "Handle: handle should call CreateAsync method with AuthenticationProviderFactory.")]
    public async Task Is_Handle_Calls_CreateAsync_With_Provider()
    {
        // Let
        var request = new RegisterAccountCommand
        {
            AuthenticationProvider = AuthenticationProvider.Self,
            Email = "kangdroid@test.com",
            Nickname = "kangdroid",
            AuthCode = "testPassword@"
        };
        var mockAccount = new Core.Models.Data.Account
        {
            Id = Ulid.NewUlid().ToString(),
            NickName = request.Nickname,
            Email = request.Email
        };
        _mockRegisterService.Setup(a => a.CreateAccountAsync(request))
                            .ReturnsAsync(mockAccount);
        _mockMediator.Setup(a => a.Send(It.IsAny<ProvisionRootByIdCommand>(), It.IsAny<CancellationToken>()))
                     .Callback((IRequest<string> request, CancellationToken _) =>
                     {
                         Assert.True(request is ProvisionRootByIdCommand);
                         var command = request as ProvisionRootByIdCommand;
                         Assert.NotNull(command);
                         Assert.Equal(mockAccount.Id, command.AccountId);
                     })
                     .ReturnsAsync("rootId");
        _mockJwtService.Setup(a => a.GenerateJwt(It.IsAny<List<Claim>>(), null))
                       .Returns("accessToken");
        _mockJwtService.Setup(a => a.GenerateJwt(null, It.IsAny<DateTime>()))
                       .Returns("refreshToken");
        _mockCacheService.Setup(a =>
            a.SetItemAsync(AccountCacheKeys.RefreshTokenKey("refreshToken"), It.IsAny<object>(), It.IsAny<TimeSpan>()));

        // Do
        var response = await _commandHandler.Handle(request, default);

        // Verify
        _mockRegisterService.VerifyAll();
        _mockMediator.VerifyAll();
        _mockJwtService.VerifyAll();
        _mockCacheService.VerifyAll();

        // Check
        Assert.Equal("accessToken", response.AccessToken);
        Assert.Equal("refreshToken", response.RefreshToken);
    }
}