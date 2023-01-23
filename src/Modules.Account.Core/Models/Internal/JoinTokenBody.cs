using System.IdentityModel.Tokens.Jwt;
using Modules.Account.Core.Models.Data;
using Shared.Core.Services;

namespace Modules.Account.Core.Models.Internal;

public class JoinTokenBody
{
    public string Id { get; set; }
    public string Email { get; set; }
    public AuthenticationProvider Provider { get; set; }

    public static JoinTokenBody CreateFromJwt(string jwt)
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
}