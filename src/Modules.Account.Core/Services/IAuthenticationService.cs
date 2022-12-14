using Modules.Account.Core.Commands;
using Modules.Account.Core.Models.Data;
using Modules.Account.Core.Models.Responses;

namespace Modules.Account.Core.Services;

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
    ///     Create Account with given Register Account Command(Register Request)
    /// </summary>
    /// <remarks>
    ///     Every provider should implement account creation method with it's OAuth2 Policy.
    /// </remarks>
    /// <param name="registerAccountCommand">Register Request</param>
    /// <returns>Created Account</returns>
    public Task<Models.Data.Account> CreateAccountAsync(RegisterAccountCommand registerAccountCommand);

    /// <summary>
    ///     Login to KDRFC Server.
    /// </summary>
    /// <param name="loginCommand">Login Request</param>
    /// <returns>Access Token Response containing AccessToken, RefreshToken.</returns>
    public Task<AccessTokenResponse> LoginAsync(LoginCommand loginCommand);
}