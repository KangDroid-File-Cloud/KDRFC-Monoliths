using MediatR;
using Modules.Account.Core.Extensions;
using Modules.Account.Core.Models.Data;
using Modules.Account.Core.Models.Responses;
using Modules.Account.Core.Services.Authentication;
using Shared.Core.Abstractions;
using Shared.Core.Commands;
using Shared.Core.Services;

namespace Modules.Account.Core.Commands.Handlers;

public class LoginAccountCommandHandler : IRequestHandler<LoginCommand, AccessTokenResponse>
{
    private readonly AuthenticationProviderFactory _authenticationProviderFactory;
    private readonly ICacheService _cacheService;
    private readonly IJwtService _jwtService;
    private readonly IMediator _mediator;

    public LoginAccountCommandHandler(AuthenticationProviderFactory factory, IMediator mediator, ICacheService cacheService,
                                      IJwtService jwtService)
    {
        _authenticationProviderFactory = factory;
        _mediator = mediator;
        _cacheService = cacheService;
        _jwtService = jwtService;
    }

    public async Task<AccessTokenResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var providerFactory = _authenticationProviderFactory(request.AuthenticationProvider);

        // Get Credential(Authenticate through providers)
        var credential = await providerFactory.AuthenticateAsync(request);

        // Get Root
        var rootId = await _mediator.Send(new GetRootByAccountIdCommand
        {
            AccountId = credential.Account.Id
        }, cancellationToken);

        // Create JWT
        var jwt = _jwtService.GenerateAccessToken(credential.Account, credential.AuthenticationProvider, rootId);

        // Create Refresh Token
        var refreshToken = new RefreshToken
        {
            UserId = credential.UserId,
            Token = _jwtService.GenerateJwt(null, DateTime.UtcNow.AddDays(14))
        };
        await _cacheService.SetItemAsync(AccountCacheKeys.RefreshTokenKey(refreshToken.Token), refreshToken,
            TimeSpan.FromDays(14));

        return new AccessTokenResponse
        {
            AccessToken = jwt,
            RefreshToken = refreshToken.Token
        };
    }
}