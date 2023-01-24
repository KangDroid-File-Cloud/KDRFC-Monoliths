using MediatR;
using Modules.Account.Core.Extensions;
using Modules.Account.Core.Models.Data;
using Modules.Account.Core.Models.Responses;
using Modules.Account.Core.Services.Register;
using Shared.Core.Abstractions;
using Shared.Core.Commands;
using Shared.Core.Services;

namespace Modules.Account.Core.Commands.Handlers;

public class RegisterAccountCommandHandler : IRequestHandler<RegisterAccountCommand, AccessTokenResponse>
{
    private readonly ICacheService _cacheService;
    private readonly IJwtService _jwtService;
    private readonly IMediator _mediator;
    private readonly RegisterProviderFactory _registerProviderFactory;

    public RegisterAccountCommandHandler(IMediator mediator, RegisterProviderFactory registerProviderFactory,
                                         IJwtService jwtService, ICacheService cacheService)
    {
        _mediator = mediator;
        _registerProviderFactory = registerProviderFactory;
        _jwtService = jwtService;
        _cacheService = cacheService;
    }

    public async Task<AccessTokenResponse> Handle(RegisterAccountCommand request, CancellationToken cancellationToken)
    {
        var providerFactory = _registerProviderFactory(request.AuthenticationProvider);

        // Create Account(May throw ApiException)
        var account = await providerFactory.CreateAccountAsync(request);

        // Make sure root file system provisioned correctly.
        var rootId = await _mediator.Send(new ProvisionRootByIdCommand
        {
            AccountId = account.Id
        }, cancellationToken);

        // Create JWT
        var jwt = _jwtService.GenerateAccessToken(account, request.AuthenticationProvider, rootId);

        // Create Refresh Token
        var refreshToken = new RefreshToken
        {
            UserId = account.Id,
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