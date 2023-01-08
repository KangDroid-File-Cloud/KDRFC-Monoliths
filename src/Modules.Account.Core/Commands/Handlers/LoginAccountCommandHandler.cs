using MediatR;
using Modules.Account.Core.Models.Data;
using Modules.Account.Core.Models.Responses;
using Modules.Account.Core.Services;

namespace Modules.Account.Core.Commands.Handlers;

public class LoginAccountCommandHandler : IRequestHandler<LoginCommand, AccessTokenResponse>
{
    private readonly AuthenticationProviderFactory _authenticationProviderFactory;

    public LoginAccountCommandHandler(AuthenticationProviderFactory factory)
    {
        _authenticationProviderFactory = factory;
    }

    public async Task<AccessTokenResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var providerFactory = request.AuthenticationProvider switch
        {
            AuthenticationProvider.Self => _authenticationProviderFactory(AuthenticationProvider.Self),
            _ => throw new ArgumentException("Unknown Value", request.AuthenticationProvider.ToString())
        };

        return await providerFactory.LoginAsync(request);
    }
}