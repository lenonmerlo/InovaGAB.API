using Microsoft.AspNetCore.Mvc;

namespace InovaGAB.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (FormatException)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status400BadRequest,
                "Identificador inválido.",
                "O identificador informado não possui um formato válido.");
        }
        catch (ArgumentException exception)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status400BadRequest,
                "Dados inválidos.",
                exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status400BadRequest,
                "Operação inválida.",
                exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Erro não tratado durante a requisição.");

            await WriteProblemAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Erro interno.",
                "Ocorreu um erro interno ao processar a solicitação.");
        }
    }

    private static async Task WriteProblemAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType =
            "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        await context.Response.WriteAsJsonAsync(problem);
    }
}