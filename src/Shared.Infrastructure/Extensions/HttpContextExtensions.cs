using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Shared.Models;

namespace Shared.Infrastructure.Extensions;

public static class HttpContextExtension
{
    private const string ContextUser = "contextUser";

    /// <summary>
    ///     Set UserId to HttpContext's Item Dictionary.(Key is 'userId')
    /// </summary>
    /// <param name="context">HttpContext(Extensions)</param>
    /// <param name="userId">UserId to set.</param>
    public static void SetContextAccount(this HttpContext context, ContextAccount contextUser)
    {
        context.Items[ContextUser] = JsonConvert.SerializeObject(contextUser);
    }

    /// <summary>
    ///     Get UserId from HttpContext's Item Dictionary.(Key is 'userId')
    /// </summary>
    /// <param name="context">HttpContext(Extensions)</param>
    /// <returns>Nullable UserId from HttpContext's Item Dictionary.</returns>
    public static ContextAccount? GetContextAccount(this HttpContext context)
    {
        return JsonConvert.DeserializeObject<ContextAccount>(context.Items[ContextUser] as string ?? "");
    }
}