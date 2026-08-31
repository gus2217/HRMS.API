namespace Jacana.SharedKernel.Domain;

/// <summary>
/// Kenyan MSISDN, canonicalised to E.164 form (+254XXXXXXXXX).
/// Accepts any of the common entry formats: "+254 7…", "07…", "2547…" or the bare
/// 9-digit "7…"/"1…" — so search, lookup and registration all treat the same
/// number identically regardless of how it was typed.
/// </summary>
public sealed class PhoneNumber : ValueObject
{
    private PhoneNumber(string value) => Value = value;

    public string Value { get; }

    /// <summary>
    /// Normalises a user-typed phone string to E.164, or null when it cannot be
    /// interpreted as a Kenyan number. Strips spaces, dashes, parentheses and a
    /// leading "+", then resolves 0-prefixed, 254-prefixed and bare 9-digit forms.
    /// </summary>
    public static string? TryNormalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;

        var digits = new string(input.Where(char.IsDigit).ToArray());
        if (digits.Length == 0) return null;

        // Resolve to the 9-digit national significant number.
        if (digits.StartsWith("254", StringComparison.Ordinal) && digits.Length == 12)
            digits = digits[3..];
        else if (digits.StartsWith("0", StringComparison.Ordinal) && digits.Length == 10)
            digits = digits[1..];

        if (digits.Length != 9) return null;
        if (digits[0] is not ('7' or '1')) return null;

        return "+254" + digits;
    }

    public static Result<PhoneNumber> Create(string? input)
    {
        var normalized = TryNormalize(input);
        return normalized is null
            ? Error.Validation($"'{input}' is not a valid Kenyan phone number.")
            : new PhoneNumber(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
