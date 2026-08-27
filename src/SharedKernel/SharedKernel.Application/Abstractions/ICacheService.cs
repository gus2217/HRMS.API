namespace Jacana.SharedKernel.Application.Abstractions;

/// <summary>
/// Distributed cache abstraction. Redis-backed in production; a memory-cached
/// implementation ships first and can be swapped via DI registration.
/// </summary>
public interface ICacheService
{
    Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
    Task InvalidateByTagAsync(string tag, CancellationToken ct = default);
}
