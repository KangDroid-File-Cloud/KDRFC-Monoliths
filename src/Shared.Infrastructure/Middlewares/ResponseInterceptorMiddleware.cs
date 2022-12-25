using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;

namespace Shared.Infrastructure.Middlewares;

[ExcludeFromCodeCoverage]
public class ResponseInterceptorMiddleware
{
    private readonly RequestDelegate _next;

    public ResponseInterceptorMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        // Backup Original Response Body
        var originalResponseBody = context.Response.Body;

        // Use our Response Body
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        // Do work
        await _next(context);

        // Rewind
        memoryStream.Position = 0;
        var responseBodyStr = await new StreamReader(memoryStream).ReadToEndAsync();
        context.Items["ResponseBody"] = responseBodyStr;

        // Rewind to begin again and copy to original response body
        memoryStream.Position = 0;
        await memoryStream.CopyToAsync(originalResponseBody);
    }
}