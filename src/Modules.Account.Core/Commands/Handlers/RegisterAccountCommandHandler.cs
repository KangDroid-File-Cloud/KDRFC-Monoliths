using MediatR;
using Modules.Account.Core.Models.Data;
using Modules.Account.Core.Services;
using Shared.Core.Commands;

namespace Modules.Account.Core.Commands.Handlers;

public class RegisterAccountCommandHandler : IRequestHandler<RegisterAccountCommand>
{
    private readonly AuthenticationProviderFactory _authenticationProviderFactory;
    private readonly IMediator _mediator;

    public RegisterAccountCommandHandler(AuthenticationProviderFactory factory, IMediator mediator)
    {
        _authenticationProviderFactory = factory;
        _mediator = mediator;
    }

    public async Task<Unit> Handle(RegisterAccountCommand request, CancellationToken cancellationToken)
    {
        var providerFactory = request.AuthenticationProvider switch
        {
            AuthenticationProvider.Self => _authenticationProviderFactory(AuthenticationProvider.Self),
            _ => throw new ArgumentException("Unknown Value", request.AuthenticationProvider.ToString())
        };

        // Create Account(May throw ApiException)
        var account = await providerFactory.CreateAccountAsync(request);

        // Make sure root file system provisioned correctly.
        await _mediator.Send(new ProvisionRootByIdCommand
        {
            AccountId = account.Id
        }, cancellationToken);

        return Unit.Value;
    }
}