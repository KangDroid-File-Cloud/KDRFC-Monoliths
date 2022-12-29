using MediatR;
using Modules.Account.Core.Commands;
using Modules.Account.Core.Models.Data;
using Modules.Account.Core.Services;
using Moq;
using Shared.Core.Commands;
using Xunit;

namespace Modules.Account.Test.Commands;

public class RegisterAccountCommandTest
{
    private readonly AuthenticationProviderFactory _authenticationProviderFactory;
    private readonly RegisterAccountCommandHandler _commandHandler;

    // Authentication Providers
    private readonly Mock<IAuthenticationService> _mockAuthenticationService;

    // IMediator
    private readonly Mock<IMediator> _mockMediator;

    public RegisterAccountCommandTest()
    {
        _mockAuthenticationService = new Mock<IAuthenticationService>();
        _authenticationProviderFactory = provider => _mockAuthenticationService.Object;
        _mockMediator = new Mock<IMediator>();
        _commandHandler = new RegisterAccountCommandHandler(_authenticationProviderFactory, _mockMediator.Object);
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
            Id = Ulid.NewUlid().ToString()
        };
        _mockAuthenticationService.Setup(a => a.CreateAccountAsync(request))
                                  .ReturnsAsync(mockAccount);
        _mockMediator.Setup(a => a.Send(It.IsAny<ProvisionRootByIdCommand>(), It.IsAny<CancellationToken>()))
                     .Callback((IRequest<Unit> request, CancellationToken _) =>
                     {
                         Assert.True(request is ProvisionRootByIdCommand);
                         var command = request as ProvisionRootByIdCommand;
                         Assert.NotNull(command);
                         Assert.Equal(mockAccount.Id, command.AccountId);
                     });

        // Do
        await _commandHandler.Handle(request, default);

        // Verify
        _mockAuthenticationService.VerifyAll();
        _mockMediator.VerifyAll();
    }
}