using MediatR;
using Modules.Account.Core.Models.Data;

namespace Modules.Account.Core.Commands;

public class RegisterAccountCommand : IRequest
{
    /// <summary>
    ///     Nickname of User.
    /// </summary>
    /// <example>KangDroid</example>
    public string Nickname { get; set; }

    /// <summary>
    ///     Email of User
    /// </summary>
    /// <example>kangdroid@testhelloworld.com</example>
    public string Email { get; set; }

    /// <summary>
    ///     Authentication Provider - Self, OAuth, etc.
    /// </summary>
    /// <example>Self</example>
    public AuthenticationProvider AuthenticationProvider { get; set; }

    /// <summary>
    ///     Authentication Code.(Password when self.)
    /// </summary>
    public string AuthCode { get; set; }
}