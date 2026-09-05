using Jacana.SharedKernel.Application.Abstractions;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace Jacana.HRMS.Api.Storage;

/// <summary>
/// S3/MinIO-backed file storage behind <see cref="IFileStorage"/>. Keys map
/// 1:1 to object names in the configured bucket; the bucket is created on
/// first use so no out-of-band setup is required. Endpoint/credentials are
/// supplied from configuration ("Storage:Minio").
/// </summary>
public sealed class MinioFileStorage : IFileStorage
{
    private readonly IMinioClient _client;
    private readonly string _bucket;
    private readonly SemaphoreSlim _ensureLock = new(1, 1);
    private bool _bucketReady;

    public MinioFileStorage(string endpoint, string accessKey, string secretKey, string bucket, bool useSsl = false)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("MinIO endpoint is required (Storage:Minio:Endpoint).", nameof(endpoint));
        if (string.IsNullOrWhiteSpace(bucket))
            throw new ArgumentException("MinIO bucket is required (Storage:Minio:Bucket).", nameof(bucket));

        _bucket = bucket;
        _client = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey ?? string.Empty, secretKey ?? string.Empty)
            .WithSSL(useSsl)
            .Build();
    }

    public async Task SaveAsync(string key, byte[] content, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        await EnsureBucketAsync(ct);

        using var stream = new MemoryStream(content, writable: false);
        await _client.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(_bucket)
                .WithObject(key)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(GuessContentType(key)),
            ct);
    }

    public async Task<byte[]?> ReadAsync(string key, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        await EnsureBucketAsync(ct);

        using var output = new MemoryStream();
        try
        {
            await _client.GetObjectAsync(
                new GetObjectArgs()
                    .WithBucket(_bucket)
                    .WithObject(key)
                    .WithCallbackStream(s => s.CopyTo(output)),
                ct);
            return output.ToArray();
        }
        catch (ObjectNotFoundException)
        {
            return null;
        }
    }

    public async Task DeleteAsync(string key, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        await EnsureBucketAsync(ct);

        try
        {
            await _client.RemoveObjectAsync(
                new RemoveObjectArgs().WithBucket(_bucket).WithObject(key), ct);
        }
        catch (ObjectNotFoundException)
        {
            // Nothing to delete — treat as success (same contract as local disk).
        }
    }

    private async Task EnsureBucketAsync(CancellationToken ct)
    {
        if (_bucketReady) return;

        await _ensureLock.WaitAsync(ct);
        try
        {
            if (_bucketReady) return;

            var exists = await _client.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(_bucket), ct);
            if (!exists)
            {
                await _client.MakeBucketAsync(
                    new MakeBucketArgs().WithBucket(_bucket), ct);
            }

            _bucketReady = true;
        }
        finally
        {
            _ensureLock.Release();
        }
    }

    private static string GuessContentType(string key)
    {
        var ext = Path.GetExtension(key).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".svg" => "image/svg+xml",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            ".mp4" => "video/mp4",
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            _ => "application/octet-stream"
        };
    }
}
