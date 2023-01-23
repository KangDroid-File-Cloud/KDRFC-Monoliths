using Modules.Account.Core.Commands;
using Modules.Account.Core.Models.Data;

namespace Modules.Account.Core.Services.Register;

public delegate IRegisterService RegisterProviderFactory(AuthenticationProvider provider);

public interface IRegisterService
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
}