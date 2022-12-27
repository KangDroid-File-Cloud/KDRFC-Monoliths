using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Shared.Core.Services;
using Shared.Infrastructure.Extensions;
using Shared.Models.Responses;

namespace Shared.Infrastructure.Filters;

public class KDRFCAuthorizationAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Get Access Token
        var accessToken = context.HttpContext.Request.GetAuthorizationParameter();

        // Case 1. Access Token does not exists.
        if (accessToken == null)
        {
            context.Result = new UnauthorizedObjectResult(new ErrorResponse
            {
                Message = "API Authorization Failed.",
                TraceIdentifier = context.HttpContext.TraceIdentifier
            });
            return;
        }

        // Case 2. AccessToken does exists, but cannot validate.
        var jwtService = context.HttpContext.RequestServices.GetService<IJwtService>();
        var jwt = jwtService.ValidateJwt(accessToken);
        if (jwt == null)
        {
            context.Result = new UnauthorizedObjectResult(new ErrorResponse
            {
                Message = "API Authorization Failed.",
                TraceIdentifier = context.HttpContext.TraceIdentifier
            });
            return;
        }

        // Case 3. Access Token exists, validate succeed.
        var userId = jwt.Claims.First(a => a.Type == JwtRegisteredClaimNames.Sub).Value;
        context.HttpContext.SetUserId(userId);

        await next();
    }
}