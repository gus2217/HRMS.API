namespace Jacana.SharedKernel.Application.Abstractions;

/// <summary>
/// Abstraction over physical document/blob storage. The application layer works
/// with byte arrays and opaque keys; the infrastructure decides where bytes live
/// (local disk, object storage, etc.). Keys are caller-supplied and unique.
/// </summary>
public interface IFileStorage
{
    Task SaveAsync(string key, byte[] content, CancellationToken ct = default);
    Task<byte[]?> ReadAsync(string key, CancellationToken ct = default);
    Task DeleteAsync(string key, CancellationToken ct = default);
}
