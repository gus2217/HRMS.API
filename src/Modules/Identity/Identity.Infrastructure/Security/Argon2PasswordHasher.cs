using System.Security.Cryptography;
using System.Text;
using Jacana.Identity.Application.Abstractions;
using Konscious.Security.Cryptography;

namespace Jacana.Identity.Infrastructure.Security;

/// <summary>
/// Argon2id password hashing. Work factor: memory 64 MiB, iterations 3, parallelism 2,
/// 16-byte salt. Output format: argon2id$m=65536,t=3,p=2$base64(salt)$base64(hash).
/// </summary>
public sealed class Argon2PasswordHasher : IPasswordHasher
{
    private const int MemorySizeKiB = 65536; // 64 MiB
    private const int Iterations = 3;
    private const int Parallelism = 2;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = ComputeHash(password, salt, MemorySizeKiB, Iterations, Parallelism);
        return $"argon2id$m={MemorySizeKiB},t={Iterations},p={Parallelism}$" +
               $"{Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string stored)
    {
        try
        {
            var parts = stored.Split('$');
            if (parts.Length != 4 || parts[0] != "argon2id") return false;

            var parameters = parts[1].Split(',');
            var memory = int.Parse(parameters[0]["m=".Length..]);
            var iterations = int.Parse(parameters[1]["t=".Length..]);
            var parallelism = int.Parse(parameters[2]["p=".Length..]);

            var salt = Convert.FromBase64String(parts[2]);
            var expected = Convert.FromBase64String(parts[3]);

            var actual = ComputeHash(password, salt, memory, iterations, parallelism);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch
        {
            return false;
        }
    }

    private static byte[] ComputeHash(string password, byte[] salt, int memoryKiB, int iterations, int parallelism)
    {
        using var argon = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            MemorySize = memoryKiB,
            Iterations = iterations,
            DegreeOfParallelism = parallelism
        };
        return argon.GetBytes(HashSize);
    }
}
