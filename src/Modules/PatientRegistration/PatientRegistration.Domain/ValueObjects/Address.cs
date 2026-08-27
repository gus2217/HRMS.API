using Jacana.SharedKernel.Domain;

namespace Jacana.PatientRegistration.Domain;

/// <summary>Patient physical address. County is the minimum required component.</summary>
public sealed class Address : ValueObject
{
    private Address(string county, string? subCounty, string? ward, string? line1)
    {
        County = county;
        SubCounty = subCounty;
        Ward = ward;
        Line1 = line1;
    }

    public string County { get; }
    public string? SubCounty { get; }
    public string? Ward { get; }
    public string? Line1 { get; }

    public static Result<Address> Create(string county, string? subCounty = null, string? ward = null, string? line1 = null)
    {
        if (string.IsNullOrWhiteSpace(county))
            return Error.Validation("County is required.");
        return new Address(county.Trim(), NullIfEmpty(subCounty), NullIfEmpty(ward), NullIfEmpty(line1));
    }

    private static string? NullIfEmpty(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return County;
        yield return SubCounty;
        yield return Ward;
        yield return Line1;
    }
}
