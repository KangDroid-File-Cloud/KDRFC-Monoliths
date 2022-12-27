using Microsoft.AspNetCore.Http;

namespace Shared.Infrastructure.Extensions;

public static class HttpContextExtension
{
    private const string UserId = "userId";

    /// <summary>
    ///     Set UserId to HttpContext's Item Dictionary.(Key is 'userId')
    /// </summary>
    /// <param name="context">HttpContext(Extensions)</param>
    /// <param name="userId">UserId to set.</param>
    public static void SetUserId(this HttpContext context, string userId)
    {
        context.Items[UserId] = userId;
    }

    /// <summary>
    ///     Get UserId from HttpContext's Item Dictionary.(Key is 'userId')
    /// </summary>
    /// <param name="context">HttpContext(Extensions)</param>
    /// <returns>Nullable UserId from HttpContext's Item Dictionary.</returns>
    public static string? GetUserId(this HttpContext context)
    {
        return context.Items[UserId] as string;
    }
}