using System.ComponentModel.DataAnnotations;
using MediatR;
using Modules.Account.Core.Models.Data;
using Modules.Account.Core.Models.Responses;

namespace Modules.Account.Core.Commands;

public class RegisterAccountCommand : IRequest<AccessTokenResponse>
{
    /// <summary>
    ///     Nickname of User.
    /// </summary>
    /// <example>KangDroid</example>
    [Required]
    public string Nickname { get; set; }

    /// <summary>
    ///     Email of User
    /// </summary>
    /// <example>kangdroid@testhelloworld.com</example>
    [Required]
    public string Email { get; set; }

    /// <summary>
    ///     Authentication Provider - Self, OAuth, etc.
    /// </summary>
    /// <example>Self</example>
    [Required]
    public AuthenticationProvider AuthenticationProvider { get; set; }

    /// <summary>
    ///     Authentication Code.(Password when self.)
    /// </summary>
    [Required]
    public string AuthCode { get; set; }
}