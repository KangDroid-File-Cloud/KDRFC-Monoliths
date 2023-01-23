using Modules.Account.Core.Commands;
using Modules.Account.Core.Models.Data;

namespace Modules.Account.Core.Services.Authentication;

/// <summary>
///     Authentication Provider Factory(Registered in DI Container.)
/// </summary>
public delegate IAuthenticationService AuthenticationProviderFactory(AuthenticationProvider provider);

/// <summary>
///     Authentication Service Interface. Designated to support multiple provider(i.e self, google, etc)
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    ///     Login to KDRFC Server.
    /// </summary>
    /// <param name="loginCommand">Login Request</param>
    /// <returns>Access Token Response containing AccessToken, RefreshToken.</returns>
    public Task<Credential> AuthenticateAsync(LoginCommand loginCommand);
}