using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace Shared.Infrastructure.Extensions;

public static class HttpRequestExtension
{
    /// <summary>
    ///     Extension method to get authorization 'parameter' from HttpRequest.
    /// </summary>
    /// <param name="request">HttpRequest(Extension)</param>
    /// <returns>Nullable Authorization Header's Parameter, i.e AccessTokens.</returns>
    public static string? GetAuthorizationParameter(this HttpRequest request)
    {
        AuthenticationHeaderValue.TryParse(request.Headers.Authorization, out var headerValue);

        return headerValue?.Parameter;
    }
}