using System.Diagnostics;
using System.Security.Claims;
using InovaGAB.API.Data;
using InovaGAB.API.Models;

namespace InovaGAB.API.Middleware;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;

    public AuditMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
    {
        // Ignora swagger e health
        var path = context.Request.Path.Value ?? string.Empty;
        if (path.Contains("/swagger") || path.Contains("/health"))
        {
            await _next(context);
            return;
        }

        var sw = Stopwatch.StartNew();
        await _next(context);
        sw.Stop();

        var userEmail = context.User?.FindFirstValue(ClaimTypes.Email);
        var userRole = context.User?.FindFirstValue(ClaimTypes.Role);

        var log = new AuditLog
        {
            Method = context.Request.Method,
            Endpoint = path,
            UserEmail = userEmail,
            UserRole = userRole,
            StatusCode = context.Response.StatusCode,
            DurationMs = sw.ElapsedMilliseconds,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.AuditLogs.Add(log);
        await dbContext.SaveChangesAsync();
    }
}