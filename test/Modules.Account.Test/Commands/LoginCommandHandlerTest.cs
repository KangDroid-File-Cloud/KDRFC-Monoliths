using Modules.Account.Core.Commands;
using Modules.Account.Core.Commands.Handlers;
using Modules.Account.Core.Models.Data;
using Modules.Account.Core.Models.Responses;
using Modules.Account.Core.Services;
using Moq;
using Xunit;

namespace Modules.Account.Test.Commands;

public class LoginCommandHandlerTest
{
    private readonly AuthenticationProviderFactory _authenticationProviderFactory;
    private readonly LoginAccountCommandHandler _commandHandler;

    // Authentication Providers
    private readonly Mock<IAuthenticationService> _mockAuthenticationService;

    public LoginCommandHandlerTest()
    {
        _mockAuthenticationService = new Mock<IAuthenticationService>();
        _authenticationProviderFactory = provider => _mockAuthenticationService.Object;
        _commandHandler = new LoginAccountCommandHandler(_authenticationProviderFactory);
    }

    [Fact(DisplayName = "Handle: handle should call LoginAsync method with AuthenticationProviderFactory.")]
    public async Task Is_Handle_Calls_CreateAsync_With_Provider()
    {
        // Let
        var request = new LoginCommand
        {
            AuthenticationProvider = AuthenticationProvider.Self,
            AuthCode = "testPassword@",
            Email = "kangdroid@test.com"
        };
        _mockAuthenticationService.Setup(a => a.LoginAsync(request))
                                  .ReturnsAsync(new AccessTokenResponse());

        // Do
        await _commandHandler.Handle(request, default);

        // Verify
        _mockAuthenticationService.VerifyAll();
    }
}