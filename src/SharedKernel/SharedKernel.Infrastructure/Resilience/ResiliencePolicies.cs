using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace Jacana.SharedKernel.Infrastructure.Resilience;

/// <summary>
/// Central Polly resilience strategies. Every external gateway call (SHA, M-Pesa,
/// SMS/WhatsApp) must use these — never a bare HttpClient.SendAsync.
/// </summary>
public static class ResiliencePolicies
{
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(15);

    /// <summary>Retry (exponential backoff + jitter) for transient network/5xx failures.</summary>
    public static RetryStrategyOptions ExternalHttpRetry() => new()
    {
        MaxRetryAttempts = 3,
        BackoffType = DelayBackoffType.Exponential,
        Delay = TimeSpan.FromMilliseconds(500),
        UseJitter = true,
        OnRetry = _ => ValueTask.CompletedTask
    };

    /// <summary>Circuit breaker: opens after 5 consecutive failures, half-opens after 30s.</summary>
    public static CircuitBreakerStrategyOptions ExternalHttpCircuitBreaker() => new()
    {
        FailureRatio = 0.5,
        MinimumThroughput = 5,
        SamplingDuration = TimeSpan.FromSeconds(30),
        BreakDuration = TimeSpan.FromSeconds(30),
        OnOpened = _ => ValueTask.CompletedTask,
        OnClosed = _ => ValueTask.CompletedTask,
        OnHalfOpened = _ => ValueTask.CompletedTask
    };

    /// <summary>Explicit timeout — no unbounded outbound waits.</summary>
    public static TimeoutStrategyOptions ExternalHttpTimeout() => new()
    {
        Timeout = DefaultTimeout
    };
}
