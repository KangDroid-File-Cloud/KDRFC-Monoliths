using System.Net;

namespace Shared.Core.Exceptions;

public class ApiException : Exception
{
    public int StatusCode { get; set; }
    public override string Message { get; }

    public ApiException(HttpStatusCode statusCode, string message)
    {
        StatusCode = (int)statusCode;
        Message = message;
    }
}