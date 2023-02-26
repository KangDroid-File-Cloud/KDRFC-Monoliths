using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Shared.Infrastructure.Filters;

public class SwaggerOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var endpointAuthorizationEnabled = context.ApiDescription.ActionDescriptor.EndpointMetadata
                                                  .Any(a => a.GetType() == typeof(KDRFCAuthorizationAttribute));

        if (endpointAuthorizationEnabled)
        {
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "KDRFCAuthorization"
                        }
                    },
                    new List<string>()
                }
            });
        }
    }
}