using Jacana.SharedKernel.Application.Abstractions;

namespace Jacana.SharedKernel.Infrastructure.Services;

/// <summary>
/// Local-disk file storage. Keys are treated as relative paths under a root
/// directory; path segments are sanitised so a caller-supplied key cannot escape
/// the root. Suitable for a single-node deployment; swap for object storage
/// (S3/GCS) by implementing <see cref="IFileStorage"/> behind the same interface.
/// </summary>
public sealed class LocalFileStorage : IFileStorage
{
    private readonly string _root;

    public LocalFileStorage(string rootPath)
    {
        _root = Path.GetFullPath(rootPath);
    }

    public async Task SaveAsync(string key, byte[] content, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var path = Resolve(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllBytesAsync(path, content, ct);
    }

    public async Task<byte[]?> ReadAsync(string key, CancellationToken ct = default)
    {
        var path = Resolve(key);
        return File.Exists(path) ? await File.ReadAllBytesAsync(path, ct) : null;
    }

    public Task DeleteAsync(string key, CancellationToken ct = default)
    {
        var path = Resolve(key);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    /// <summary>Resolves a key to an absolute path and guarantees it stays under root.</summary>
    private string Resolve(string key)
    {
        var full = Path.GetFullPath(Path.Combine(_root, key));
        var rootWithSep = _root.EndsWith(Path.DirectorySeparatorChar)
            ? _root
            : _root + Path.DirectorySeparatorChar;

        if (!full.StartsWith(rootWithSep, StringComparison.Ordinal))
            throw new InvalidOperationException("Invalid storage key.");

        return full;
    }
}
