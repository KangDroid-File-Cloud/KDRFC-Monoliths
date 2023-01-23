using MediatR;
using Modules.Account.Core.Services.Register;
using Shared.Core.Commands;

namespace Modules.Account.Core.Commands.Handlers;

public class RegisterAccountCommandHandler : IRequestHandler<RegisterAccountCommand>
{
    private readonly IMediator _mediator;
    private readonly RegisterProviderFactory _registerProviderFactory;

    public RegisterAccountCommandHandler(IMediator mediator, RegisterProviderFactory registerProviderFactory)
    {
        _mediator = mediator;
        _registerProviderFactory = registerProviderFactory;
    }

    public async Task<Unit> Handle(RegisterAccountCommand request, CancellationToken cancellationToken)
    {
        var providerFactory = _registerProviderFactory(request.AuthenticationProvider);

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