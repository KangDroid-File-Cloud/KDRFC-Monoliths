using MediatR;
using Modules.Account.Core.Models.Data;
using Modules.Account.Core.Models.Responses;
using Modules.Account.Core.Services;

namespace Modules.Account.Core.Commands;

public class LoginCommand : IRequest<AccessTokenResponse>
{
    /// <summary>
    ///     Authentication Provider
    /// </summary>
    public AuthenticationProvider AuthenticationProvider { get; set; }

    /// <summary>
    ///     Email Address(Only applies when self authentication provider)
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    ///     Authentication ID(OAuth ID when OAuth, Password when Self)
    /// </summary>
    public string AuthCode { get; set; }
}

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