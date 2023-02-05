using System.ComponentModel.DataAnnotations;
using Modules.Account.Core.Commands;
using Modules.Account.Core.Models.Data;

namespace Modules.Account.Core.Models.Requests;

public class LoginRequest
{
    /// <summary>
    ///     Authentication Provider
    /// </summary>
    [Required]
    public AuthenticationProvider AuthenticationProvider { get; set; }

    /// <summary>
    ///     Email Address(Only applies when self authentication provider)
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    ///     Authentication ID(OAuth ID when OAuth, Password when Self)
    /// </summary>
    [Required]
    public string AuthCode { get; set; }

    public LoginCommand ToLoginCommand(string requestHost)
    {
        return new()
        {
            AuthenticationProvider = AuthenticationProvider,
            Email = Email,
            AuthCode = AuthCode,
            RequestHost = requestHost
        };
    }
}