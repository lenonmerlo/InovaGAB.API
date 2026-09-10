using System.Diagnostics;
using System.Security.Claims;
using InovaGAB.API.Data;
using InovaGAB.API.Models;

namespace InovaGAB.API.Middleware;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditMiddleware> _logger;

    public AuditMiddleware(
        RequestDelegate next,
        ILogger<AuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        MongoDbContext dbContext)
    {
        var path =
            context.Request.Path.Value ?? string.Empty;

        if (path.Contains(
                "/swagger",
                StringComparison.OrdinalIgnoreCase) ||
            path.Contains(
                "/health",
                StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var statusCode = StatusCodes.Status500InternalServerError;

        try
        {
            await _next(context);
            statusCode = context.Response.StatusCode;
        }
        finally
        {
            stopwatch.Stop();

            var userEmail = context.User?
                .FindFirstValue(ClaimTypes.Email);

            var userRole = context.User?
                .FindFirstValue(ClaimTypes.Role);

            var auditLog = new AuditLog
            {
                Method = context.Request.Method,
                Endpoint = path,
                UserEmail = userEmail,
                UserRole = userRole,
                StatusCode = statusCode,
                DurationMs = stopwatch.ElapsedMilliseconds,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                await dbContext.AuditLogs
                    .InsertOneAsync(auditLog);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Não foi possível registrar a auditoria da requisição {Method} {Path}.",
                    context.Request.Method,
                    path);
            }
        }
    }
}
