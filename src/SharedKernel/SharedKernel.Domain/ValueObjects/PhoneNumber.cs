namespace Jacana.SharedKernel.Domain;

/// <summary>Kenyan MSISDN in E.164 form: +254XXXXXXXXX.</summary>
public sealed class PhoneNumber : ValueObject
{
    private PhoneNumber(string value) => Value = value;

    public string Value { get; }

    public static Result<PhoneNumber> Create(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Error.Validation("Phone number is required.");

        var normalized = input.Trim().Replace(" ", string.Empty);

        // Accept "07XXXXXXXX", "+2547XXXXXXXX", "2547XXXXXXXX".
        if (normalized.StartsWith("0", StringComparison.Ordinal))
            normalized = "+254" + normalized[1..];
        else if (normalized.StartsWith("254", StringComparison.Ordinal))
            normalized = "+" + normalized;

        if (normalized.Length != 13
            || !normalized.StartsWith("+254", StringComparison.Ordinal)
            || normalized[4] is not ('7' or '1')
            || !normalized[5..].All(char.IsDigit))
            return Error.Validation($"'{input}' is not a valid Kenyan phone number.");

        return new PhoneNumber(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
