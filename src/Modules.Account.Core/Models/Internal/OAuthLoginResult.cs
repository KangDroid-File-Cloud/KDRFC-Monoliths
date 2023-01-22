namespace Modules.Account.Core.Models.Internal;

/// <summary>
///     Abstracted OAuth Login Result.
/// </summary>
public class OAuthLoginResult
{
    /// <summary>
    ///     OAuth Id, provided by OAuth Provider.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     Email address, to be provided by OAuth Provider.
    /// </summary>
    public string Email { get; set; }
}