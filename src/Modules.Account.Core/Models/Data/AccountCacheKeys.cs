namespace Modules.Account.Core.Models.Data;

public static class AccountCacheKeys
{
    public static string RefreshTokenKey(string token)
    {
        return $"RefreshToken/{token}";
    }
}