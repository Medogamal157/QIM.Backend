using QIM.Application.Interfaces;
using QIM.Shared.Models;

namespace QIM.WebApi.Middleware;

public class MaintenanceModeMiddleware
{
    private readonly RequestDelegate _next;

    public MaintenanceModeMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IUnitOfWork uow)
    {
        var path = context.Request.Path.Value ?? "";

        // Always allow admin, auth, and health endpoints
        if (path.StartsWith("/api/admin", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/api/auth", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/api/health", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var setting = await uow.PlatformSettings
            .FirstOrDefaultAsync(s => s.Key == "MaintenanceMode");

        if (setting is not null && setting.Value.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = 503;
            context.Response.ContentType = "application/json";
            var result = Result.Failure("The platform is currently under maintenance. Please try again later.");
            await context.Response.WriteAsJsonAsync(result);
            return;
        }

        await _next(context);
    }
}
