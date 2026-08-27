using System.Security.Cryptography;
using Jacana.SharedKernel.Application.Abstractions;

namespace Jacana.SharedKernel.Infrastructure.Security;

/// <summary>
/// AES-256-GCM encryptor for data at rest. The 32-byte key comes from configuration
/// (secrets store in production). Output format: base64(nonce[12] || tag[16] || ciphertext).
/// </summary>
public sealed class AesGcmValueEncryptor(string base64Key) : IValueEncryptor
{
    private static readonly int NonceSize = 12;
    private static readonly int TagSize = 16;
    private readonly byte[] _key = Convert.FromBase64String(base64Key);

    public string Encrypt(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext)) return plaintext;

        var plainBytes = System.Text.Encoding.UTF8.GetBytes(plaintext);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[TagSize];
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);

        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

        var result = new byte[NonceSize + TagSize + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, result, NonceSize, TagSize);
        Buffer.BlockCopy(cipherBytes, 0, result, NonceSize + TagSize, cipherBytes.Length);
        return Convert.ToBase64String(result);
    }

    public string Decrypt(string ciphertext)
    {
        if (string.IsNullOrEmpty(ciphertext)) return ciphertext;

        var input = Convert.FromBase64String(ciphertext);
        var nonce = input[..NonceSize];
        var tag = input[NonceSize..(NonceSize + TagSize)];
        var cipherBytes = input[(NonceSize + TagSize)..];
        var plainBytes = new byte[cipherBytes.Length];

        using var aes = new AesGcm(_key, TagSize);
        aes.Decrypt(nonce, cipherBytes, tag, plainBytes);

        return System.Text.Encoding.UTF8.GetString(plainBytes);
    }
}
