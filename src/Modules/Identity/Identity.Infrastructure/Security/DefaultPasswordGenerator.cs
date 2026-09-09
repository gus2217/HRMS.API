using System.Security.Cryptography;
using Jacana.Identity.Application.Abstractions;

namespace Jacana.Identity.Infrastructure.Security;

/// <summary>
/// Cryptographically-random one-time default password. Always contains at least
/// one upper, one lower, one digit and one symbol, is 12 chars long, and excludes
/// ambiguous characters (0/O, 1/l/I) so it can be read aloud over the phone.
/// </summary>
public sealed class DefaultPasswordGenerator : IDefaultPasswordGenerator
{
    private const string Upper = "ABCDEFGHJKMNPQRSTUVWXYZ";
    private const string Lower = "abcdefghjkmnpqrstuvwxyz";
    private const string Digits = "23456789";
    private const string Symbols = "!@#$%&*";

    public string Generate()
    {
        // Guarantee one character from each class, then fill the rest randomly.
        var chars = new List<char>
        {
            Pick(Upper), Pick(Lower), Pick(Digits), Pick(Symbols),
        };
        var all = Upper + Lower + Digits + Symbols;
        while (chars.Count < 12)
            chars.Add(Pick(all));

        // Fisher–Yates shuffle with a CSPRNG so the class order is unpredictable.
        for (var i = chars.Count - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars.ToArray());
    }

    private static char Pick(string alphabet)
        => alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
}
