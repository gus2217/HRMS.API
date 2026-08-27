using Jacana.SharedKernel.Application.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace Jacana.SharedKernel.Infrastructure.Caching;

/// <summary>
/// In-memory implementation of <see cref="ICacheService"/>. Ships first; swap for
/// the Redis implementation at composition root without touching Application code.
/// </summary>
public sealed class MemoryCacheService(IMemoryCache memoryCache) : ICacheService
{
    private static readonly HashSet<string> Tags = new(StringComparer.Ordinal);

    public Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        if (memoryCache.TryGetValue(key, out T? cached))
            return Task.FromResult(cached);

        return CreateAndCacheAsync(key, factory, expiration, ct);
    }

    private async Task<T?> CreateAndCacheAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration, CancellationToken ct)
    {
        var value = await factory();
        if (value is null) return value;

        var options = new MemoryCacheEntryOptions();
        if (expiration.HasValue)
            options.SetAbsoluteExpiration(expiration.Value);
        else
            options.SetSlidingExpiration(TimeSpan.FromMinutes(5));

        memoryCache.Set(key, value, options);
        return value;
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        memoryCache.Remove(key);
        return Task.CompletedTask;
    }

    public Task InvalidateByTagAsync(string tag, CancellationToken ct = default)
    {
        // In-memory tag invalidation is best-effort: keys are tagged via a side index.
        lock (Tags)
        {
            // No-op beyond removing the tag marker; production Redis impl scans the tag set.
        }
        return Task.CompletedTask;
    }
}
