using MediatR;
using Modules.Account.Core.Models.Data;
using Modules.Account.Core.Models.Responses;

namespace Modules.Account.Core.Commands;

public class LoginCommand : IRequest<AccessTokenResponse>
{
    /// <summary>
    ///     Authentication Provider
    /// </summary>
    public AuthenticationProvider AuthenticationProvider { get; set; }

    /// <summary>
    ///     Email Address(Only applies when self authentication provider)
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    ///     Authentication ID(OAuth ID when OAuth, Password when Self)
    /// </summary>
    public string AuthCode { get; set; }

    /// <summary>
    ///     Client Request Host
    /// </summary>
    public string RequestHost { get; set; }
}