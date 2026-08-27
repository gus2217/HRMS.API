using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Application.Common;
using MediatR;

namespace Jacana.SharedKernel.Application.Behaviors;

/// <summary>Caches the result of requests implementing <see cref="ICacheableQuery"/>.</summary>
public sealed class CachingBehavior<TRequest, TResponse>(ICacheService cache)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheableQuery
    where TResponse : class
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var cached = await cache.GetOrCreateAsync(
            request.CacheKey,
            async () => await next(),
            request.Expiration,
            ct);

        return cached ?? throw new InvalidOperationException($"Cache returned null for {request.CacheKey}.");
    }
}
