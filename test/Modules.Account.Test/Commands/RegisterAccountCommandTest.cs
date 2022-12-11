using Modules.Account.Core.Commands;
using Modules.Account.Core.Models.Data;
using Modules.Account.Core.Services;
using Moq;
using Xunit;

namespace Modules.Account.Test.Commands;

public class RegisterAccountCommandTest
{
    private readonly AuthenticationProviderFactory _authenticationProviderFactory;
    private readonly RegisterAccountCommandHandler _commandHandler;

    // Authentication Providers
    private readonly Mock<IAuthenticationService> _mockAuthenticationService;

    public RegisterAccountCommandTest()
    {
        _mockAuthenticationService = new Mock<IAuthenticationService>();
        _authenticationProviderFactory = provider => _mockAuthenticationService.Object;
        _commandHandler = new RegisterAccountCommandHandler(_authenticationProviderFactory);
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
        _mockAuthenticationService.Setup(a => a.CreateAccountAsync(request))
                                  .ReturnsAsync(new Core.Models.Data.Account());

        // Do
        await _commandHandler.Handle(request, default);

        // Verify
        _mockAuthenticationService.VerifyAll();
    }
}