using System.IdentityModel.Tokens.Jwt;
using Modules.Account.Core.Commands;
using Modules.Account.Core.Models.Data;
using Modules.Account.Core.Models.Responses;
using Shared.Core.Services;

namespace Shared.Test.Helpers;

public class AccessTokenInformation
{
    public string AccountId { get; set; }
    public string RootId { get; set; }
    public string Email { get; set; }
    public string NickName { get; set; }
    public string AuthenticationProvider { get; set; }
}

public class TestUserInfo
{
    /// <summary>
    ///     Private Backing field for AccessToken Response.
    /// </summary>
    private AccessTokenResponse _accessTokenResponse;

    /// <summary>
    ///     Access Token Response(From HTTP Response)
    /// </summary>
    /// <remarks>
    ///     Setting AccessTokenResponse also sets AccessTokenInformation as well.
    /// </remarks>
    public AccessTokenResponse AccessToken
    {
        get => _accessTokenResponse;
        set
        {
            _accessTokenResponse = value;
            var jwtSecurityToken = new JwtSecurityToken(value.AccessToken);

            AccessTokenInformation = new AccessTokenInformation
            {
                AccountId = jwtSecurityToken.Claims.First(a => a.Type == JwtRegisteredClaimNames.Sub).Value,
                RootId = jwtSecurityToken.Claims.First(a => a.Type == KDRFCCommonClaimName.RootId).Value,
                Email = jwtSecurityToken.Claims.First(a => a.Type == KDRFCCommonClaimName.Email).Value,
                NickName = jwtSecurityToken.Claims.First(a => a.Type == KDRFCCommonClaimName.Nickname).Value,
                AuthenticationProvider = jwtSecurityToken.Claims
                                                         .First(a => a.Type == KDRFCCommonClaimName.AuthenticationProviderId)
                                                         .Value
            };
        }
    }

    /// <summary>
    ///     Access Token Information - for getting access token information such as JWT's Payload.
    /// </summary>
    public AccessTokenInformation AccessTokenInformation { get; set; }

    /// <summary>
    ///     A Login command to used while logging-in.
    /// </summary>
    public LoginCommand LoginCommand { get; }

    /// <summary>
    ///     A Registration Command while registering.
    /// </summary>
    /// <remarks>Automatically sets LoginCommand as well.</remarks>
    public RegisterAccountCommand RegisterAccountCommand { get; }

    public TestUserInfo(RegisterAccountCommand? registerAccountCommand = null)
    {
        RegisterAccountCommand = registerAccountCommand ?? new RegisterAccountCommand
        {
            Email = $"{Ulid.NewUlid().ToString()}@test.com",
            AuthCode = Ulid.NewUlid().ToString(),
            Nickname = "kangdroidtest",
            AuthenticationProvider = AuthenticationProvider.Self
        };
        LoginCommand = new LoginCommand
        {
            Email = RegisterAccountCommand.Email,
            AuthCode = RegisterAccountCommand.AuthCode,
            AuthenticationProvider = RegisterAccountCommand.AuthenticationProvider
        };
    }
}