using System.Security.Claims;
using Modules.Account.Core.Models.Data;
using Shared.Core.Services;

namespace Modules.Account.Core.Extensions;

public static class JwtServiceExtension
{
    public static string GenerateAccessToken(this IJwtService jwtService, Models.Data.Account account,
                                             AuthenticationProvider provider)
    {
        return jwtService.GenerateJwt(new List<Claim>
        {
            new("sub", account.Id),
            new(KDRFCCommonClaimName.AuthenticationProviderId, provider.ToString()),
            new(KDRFCCommonClaimName.Nickname, account.NickName),
            new(KDRFCCommonClaimName.Email, account.Email)
        });
    }
}