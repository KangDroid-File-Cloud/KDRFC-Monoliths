using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Shared.Core.Exceptions;
using Shared.Models.Responses;

namespace Shared.Infrastructure.Filters;

[ExcludeFromCodeCoverage]
public class GlobalExceptionFilter : ExceptionFilterAttribute
{
    private readonly ILogger _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
    {
        _logger = logger;
    }

    public override void OnException(ExceptionContext context)
    {
        // Log Error first.
        _logger.LogError(ToExceptionLogMessage(context.HttpContext.Request, context.Exception));

        if (context.Exception is ApiException exception)
        {
            context.Result = HandleApiException(exception, context.HttpContext);
        }
        else
        {
            context.Result = new ObjectResult(new ErrorResponse
            {
                Message = $"Unknown error occurred while handling request: {context.HttpContext.Request.Path}",
                TraceIdentifier = context.HttpContext.TraceIdentifier
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    private IActionResult HandleApiException(ApiException exception, HttpContext httpContext)
    {
        if (exception.CustomJsonBody != null)
        {
            return new ObjectResult(exception.CustomJsonBody)
            {
                StatusCode = exception.StatusCode
            };
        }

        return new ObjectResult(new ErrorResponse
        {
            Message = exception.Message,
            TraceIdentifier = httpContext.TraceIdentifier
        })
        {
            StatusCode = exception.StatusCode
        };
    }

    private string ToExceptionLogMessage(HttpRequest request, Exception exception)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"An error occurred while processing request ID: {request.HttpContext.TraceIdentifier}");
        stringBuilder.AppendLine($"Request: {request.HttpContext.Request.Protocol} {request.Path}");

        stringBuilder.AppendLine("Request Header Information (If Any)");
        if (request.Headers.Any())
        {
            foreach (var eachHeader in request.Headers)
            {
                stringBuilder.AppendLine($"{eachHeader.Key} : {eachHeader.Value}");
            }
        }

        stringBuilder.AppendLine($"Exception Message: {exception.Message}");
        if (exception is ApiException apiException)
        {
            stringBuilder.AppendLine($"Exception type is ApiException, StatusCode: {apiException.StatusCode}");
        }

        stringBuilder.AppendLine($"Exception StackTrace: {exception.StackTrace}");

        stringBuilder.AppendLine($"End of error log for request id: {request.HttpContext.TraceIdentifier}");

        return stringBuilder.ToString();
    }
}