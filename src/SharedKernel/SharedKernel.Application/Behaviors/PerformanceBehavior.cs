using System.Diagnostics;
using Jacana.SharedKernel.Application.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Jacana.SharedKernel.Application.Behaviors;

/// <summary>Warns when a handler exceeds a configurable threshold (default 500ms).</summary>
public sealed class PerformanceBehavior<TRequest, TResponse>(
    ILogger<PerformanceBehavior<TRequest, TResponse>> logger,
    PerformanceBehaviorOptions options)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var response = await next();
        sw.Stop();

        if (sw.ElapsedMilliseconds > options.ThresholdMs)
            logger.LogWarning(
                "Slow request {RequestName} took {ElapsedMs}ms (threshold {ThresholdMs}ms)",
                typeof(TRequest).Name, sw.ElapsedMilliseconds, options.ThresholdMs);

        return response;
    }
}

public sealed record PerformanceBehaviorOptions
{
    public long ThresholdMs { get; init; } = 500;

    public static PerformanceBehaviorOptions Default => new();
}
