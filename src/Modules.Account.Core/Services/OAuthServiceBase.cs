using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Modules.Account.Core.Abstractions;
using Modules.Account.Core.Commands;
using Modules.Account.Core.Models.Data;
using Modules.Account.Core.Models.Internal;
using Modules.Account.Core.Models.Responses;
using Shared.Core.Exceptions;
using Shared.Core.Services;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Modules.Account.Core.Services;

[ExcludeFromCodeCoverage]
public abstract class OAuthServiceBase : IAuthenticationService
{
    protected readonly IAccountDbContext _accountDbContext;
    protected readonly IJwtService _jwtService;

    protected OAuthServiceBase(IAccountDbContext accountDbContext,
                               IJwtService jwtService)
    {
        _accountDbContext = accountDbContext;
        _jwtService = jwtService;
    }

    public async Task<Models.Data.Account> CreateAccountAsync(RegisterAccountCommand registerAccountCommand)
    {
        // In OAuth Scenarios, authCode is JoinToken.
        var joinTokenBody = GetJoinTokenBodyFromJwt(registerAccountCommand.AuthCode);
        if (await _accountDbContext.Credentials.AnyAsync(
                a => a.AuthenticationProvider == registerAccountCommand.AuthenticationProvider &&
                     a.ProviderId == joinTokenBody.Id))
        {
            throw new ApiException(HttpStatusCode.Conflict,
                $"Cannot register new user: {registerAccountCommand.Email} with {registerAccountCommand.AuthenticationProvider.ToString()} already exists!");
        }

        var id = Ulid.NewUlid().ToString();
        var account = new Models.Data.Account
        {
            Id = id,
            NickName = registerAccountCommand.Nickname,
            Email = registerAccountCommand.Email,
            Credentials = new List<Credential>
            {
                new()
                {
                    UserId = id,
                    AuthenticationProvider = registerAccountCommand.AuthenticationProvider,
                    ProviderId = registerAccountCommand.Email,
                    Key = registerAccountCommand.AuthCode
                }
            }
        };
        _accountDbContext.Accounts.Add(account);
        await _accountDbContext.SaveChangesAsync(default);

        return account;
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

    private JoinTokenBody GetJoinTokenBodyFromJwt(string jwt)
    {
        var securityToken = new JwtSecurityToken(jwt);
        Enum.TryParse<AuthenticationProvider>(
            securityToken.Claims.First(a => a.Type == KDRFCCommonClaimName.AuthenticationProviderId).Value, out var provider);
        return new JoinTokenBody
        {
            Id = securityToken.Claims.First(a => a.Type == JwtRegisteredClaimNames.Sub).Value,
            Email = securityToken.Claims.First(a => a.Type == JwtRegisteredClaimNames.Email).Value,
            Provider = provider
        };
    }

    protected abstract Task<string> GetOAuthAccessTokenAsync(string authCode);
    protected abstract Task<OAuthLoginResult> GetOAuthUserInfoAsync(string accessToken);
}