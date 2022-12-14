namespace Modules.Account.Core.Models.Data;

public class RefreshToken
{
    /// <summary>
    ///     Refresh Token, Random String.
    /// </summary>
    public string Token { get; set; }

    /// <summary>
    ///     Token's Owner.
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    ///     Refresh Token Creation Date.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Refresh Token Expiration Date.
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.UtcNow.AddDays(14);
}