using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Jacana.HRMS.Api.Middleware;

/// <summary>
/// Translates domain/validation exceptions into RFC 7807 problem details.
/// Stack traces never reach the client.
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IWebHostEnvironment env)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken ct)
    {
        var problem = exception switch
        {
            ValidationException validation => Problem(
                StatusCodes.Status400BadRequest,
                "Validation failed",
                validation.Errors.Select(e => e.ErrorMessage).ToArray()),
            UnauthorizedAccessException => Problem(
                StatusCodes.Status403Forbidden, "Forbidden", []),
            _ => Problem(
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.",
                [])
        };

        context.Response.StatusCode = problem.Status!.Value;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem), ct);

        if (problem.Status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception");

        return true;
    }

    private static ProblemDetails Problem(int status, string title, string[] details)
        => new()
        {
            Status = status,
            Title = title,
            Detail = details.Length == 1 ? details[0] : string.Join("; ", details)
        };
}
