using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Modules.Account.Core.Abstractions;
using Modules.Account.Core.Commands;
using Modules.Account.Core.Models.Data;
using Modules.Account.Core.Models.Internal;
using Modules.Account.Core.Models.Responses;
using Shared.Core.Exceptions;
using Shared.Core.Services;

namespace Modules.Account.Core.Services.Authentication;

[ExcludeFromCodeCoverage]
public abstract class OAuthServiceBase : IAuthenticationService
{
    protected readonly IAccountDbContext _accountDbContext;
    protected readonly IJwtService _jwtService;

    protected OAuthServiceBase(IAccountDbContext accountDbContext, IJwtService jwtService)
    {
        _accountDbContext = accountDbContext;
        _jwtService = jwtService;
    }

    public async Task<Credential> AuthenticateAsync(LoginCommand loginCommand)
    {
        var accessToken = await GetOAuthAccessTokenAsync(loginCommand.AuthCode);
        var oauthUserInfo = await GetOAuthUserInfoAsync(accessToken);

        // Find it
        var credential = await _accountDbContext.Credentials.Include(a => a.Account)
                                                .Where(a => a.AuthenticationProvider == loginCommand.AuthenticationProvider &&
                                                            a.ProviderId == oauthUserInfo.Id)
                                                .FirstOrDefaultAsync();

        // Case - when we cannot find credential, server should generate join-token
        if (credential == null)
        {
            var joinToken = CreateJoinToken(loginCommand.AuthenticationProvider, oauthUserInfo);
            throw new ApiException(HttpStatusCode.NotFound, "", new JoinTokenResponse
            {
                JoinToken = joinToken
            });
        }

        return credential;
    }

    private string CreateJoinToken(AuthenticationProvider provider, OAuthLoginResult result)
    {
        // Create required claims
        var claims = new List<Claim>
        {
            new(KDRFCCommonClaimName.AuthenticationProviderId, provider.ToString()),
            new(JwtRegisteredClaimNames.Sub, result.Id),
            new(KDRFCCommonClaimName.Email, result.Email)
        };

        return _jwtService.GenerateJwt(claims, DateTime.UtcNow.AddMinutes(10));
    }

    protected abstract Task<string> GetOAuthAccessTokenAsync(string authCode);
    protected abstract Task<OAuthLoginResult> GetOAuthUserInfoAsync(string accessToken);
}