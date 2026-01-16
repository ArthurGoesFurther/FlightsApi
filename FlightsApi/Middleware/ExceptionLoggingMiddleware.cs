using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FlightsApi.Middleware;

public class ExceptionLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionLoggingMiddleware> _logger;

    public ExceptionLoggingMiddleware(RequestDelegate next, ILogger<ExceptionLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var user = context.User?.Identity?.Name ?? "<anonymous>";
            var time = DateTime.UtcNow;
            _logger.LogError(ex, "Unhandled exception at {Time:u} User={User} Path={Path}", time, user, context.Request.Path);

            if (!context.Response.HasStarted)
            {
                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";
                var payload = JsonSerializer.Serialize(new { error = "An unexpected error occurred." });
                await context.Response.WriteAsync(payload);
            }
        }
    }
}
