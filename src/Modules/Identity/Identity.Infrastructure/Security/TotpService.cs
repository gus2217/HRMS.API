using System.Security.Cryptography;
using System.Text;
using Jacana.Identity.Application.Abstractions;

namespace Jacana.Identity.Infrastructure.Security;

/// <summary>TOTP per RFC 6238 (SHA-1, 30s step, 6 digits). Used for opt-in 2FA.</summary>
public sealed class TotpService : ITotpService
{
    private const int StepSeconds = 30;
    private const int Digits = 6;

    public string GenerateSecret()
    {
        var bytes = RandomNumberGenerator.GetBytes(20);
        return Base32Encode(bytes);
    }

    public string GenerateQrCodeUri(string secret, string accountName)
        => $"otpauth://totp/{Uri.EscapeDataString(accountName)}?secret={secret}&issuer=JacanaHRMS&digits={Digits}&period={StepSeconds}";

    public bool Validate(string secret, string code)
    {
        if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(code) || code.Length != Digits)
            return false;

        var key = Base32Decode(secret);
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var counter = now / StepSeconds;

        // Allow one step of clock drift either way.
        return GenerateCode(key, counter) == code
            || GenerateCode(key, counter - 1) == code
            || GenerateCode(key, counter + 1) == code;
    }

    private static string GenerateCode(byte[] key, long counter)
    {
        var counterBytes = new byte[8];
        for (var i = 7; i >= 0; i--)
        {
            counterBytes[i] = (byte)(counter & 0xff);
            counter >>= 8;
        }

        var hash = new HMACSHA1(key).ComputeHash(counterBytes);
        var offset = hash[^1] & 0x0f;
        var binary = ((hash[offset] & 0x7f) << 24)
                     | ((hash[offset + 1] & 0xff) << 16)
                     | ((hash[offset + 2] & 0xff) << 8)
                     | (hash[offset + 3] & 0xff);

        var result = binary % (int)Math.Pow(10, Digits);
        return result.ToString().PadLeft(Digits, '0');
    }

    private static string Base32Encode(byte[] data)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var bits = 0;
        var value = 0;
        var output = new StringBuilder();

        foreach (var b in data)
        {
            value = (value << 8) | b;
            bits += 8;
            while (bits >= 5)
            {
                output.Append(alphabet[(value >> (bits - 5)) & 31]);
                bits -= 5;
            }
        }
        if (bits > 0)
            output.Append(alphabet[(value << (5 - bits)) & 31]);
        return output.ToString();
    }

    private static byte[] Base32Decode(string input)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var normalized = input.TrimEnd('=').ToUpperInvariant();
        var bits = 0;
        var value = 0;
        var output = new List<byte>();

        foreach (var c in normalized)
        {
            var index = alphabet.IndexOf(c);
            if (index < 0) continue;
            value = (value << 5) | index;
            bits += 5;
            if (bits >= 8)
            {
                output.Add((byte)((value >> (bits - 8)) & 0xff));
                bits -= 8;
            }
        }
        return output.ToArray();
    }
}
