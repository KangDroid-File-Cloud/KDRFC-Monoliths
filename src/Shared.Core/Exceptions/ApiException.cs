using System.Net;

namespace Shared.Core.Exceptions;

public class ApiException : Exception
{
    public int StatusCode { get; set; }
    public override string Message { get; }

    public object? CustomJsonBody { get; set; }

    public ApiException(HttpStatusCode statusCode, string message, object? customJsonBody = null)
    {
        StatusCode = (int)statusCode;
        Message = message;
        CustomJsonBody = customJsonBody;
    }
}