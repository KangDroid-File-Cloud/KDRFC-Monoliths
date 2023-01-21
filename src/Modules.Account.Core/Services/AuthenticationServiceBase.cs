using MediatR;
using Modules.Account.Core.Commands;
using Modules.Account.Core.Extensions;
using Modules.Account.Core.Models.Data;
using Modules.Account.Core.Models.Responses;
using Shared.Core.Abstractions;
using Shared.Core.Commands;
using Shared.Core.Services;

namespace Modules.Account.Core.Services;

public abstract class AuthenticationServiceBase : IAuthenticationService
{
    protected readonly ICacheService _cacheService;
    protected readonly IJwtService _jwtService;
    protected readonly IMediator _mediator;

    protected AuthenticationServiceBase(IMediator mediator, IJwtService jwtService,
                                        ICacheService cacheService)
    {
        _mediator = mediator;
        _jwtService = jwtService;
        _cacheService = cacheService;
    }

    public abstract Task<Models.Data.Account> CreateAccountAsync(RegisterAccountCommand registerAccountCommand);
    public abstract Task<AccessTokenResponse> LoginAsync(LoginCommand loginCommand);

    protected async Task<AccessTokenResponse> CreateLoginAccessTokenAsync(Credential credential)
    {
        // Get Root
        var rootId = await _mediator.Send(new GetRootByAccountIdCommand
        {
            AccountId = credential.Account.Id
        });

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