namespace Modules.Account.Core.Models.Data;

public static class AccountCacheKeys
{
    public static string RefreshTokenKey(string userId)
    {
        return $"RefreshToken/{userId}";
    }
}