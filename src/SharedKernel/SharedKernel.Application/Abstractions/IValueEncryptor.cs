namespace Jacana.SharedKernel.Application.Abstractions;

/// <summary>
/// Application-level encryption for sensitive values at rest (NationalId, ShaNumber).
/// AES-GCM with a key from the secrets store; the key itself is never in source control.
/// </summary>
public interface IValueEncryptor
{
    string Encrypt(string plaintext);
    string Decrypt(string ciphertext);
}
