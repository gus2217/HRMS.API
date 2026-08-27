using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Jacana.SharedKernel.Application.Behaviors;

/// <summary>Structured logging with correlation id; logs a warning when a handler is slow.</summary>
public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;
        var sw = Stopwatch.StartNew();

        logger.LogInformation(
            "Handling {RequestName} {@Request}",
            requestName, request);

        try
        {
            var response = await next();
            sw.Stop();
            logger.LogInformation(
                "Handled {RequestName} in {ElapsedMs}ms with outcome Success",
                requestName, sw.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(
                ex,
                "Handled {RequestName} in {ElapsedMs}ms with outcome Failure",
                requestName, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
