namespace Jacana.SharedKernel.Domain;

/// <summary>
/// Kenyan National ID number. The Infrastructure layer encrypts this at rest;
/// the value must never be logged. Plain value is held in memory only.
/// </summary>
public sealed class NationalId : ValueObject
{
    private NationalId(string value) => Value = value;

    public string Value { get; }

    public static Result<NationalId> Create(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Error.Validation("National ID is required.");

        var normalized = input.Trim().Replace(" ", string.Empty).ToUpperInvariant();

        // Kenyan IDs are digits; historically 6-8 digits, newer are 8. Allow 6-9 to
        // tolerate service numbers and legacy formats.
        if (normalized.Length is < 6 or > 9 || !normalized.All(char.IsDigit))
            return Error.Validation($"'{input}' is not a valid National ID number.");

        return new NationalId(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
