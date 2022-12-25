using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Shared.Infrastructure.Extensions;

public static class WebHostEnvironmentExtension
{
    public static bool IsTestEnvironment(this IWebHostEnvironment webHostEnvironment)
    {
        return webHostEnvironment.IsEnvironment("Development") || webHostEnvironment.IsEnvironment("LocalContainer") ||
               webHostEnvironment.IsEnvironment("Test");
    }
}