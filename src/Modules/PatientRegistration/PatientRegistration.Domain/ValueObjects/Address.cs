using Jacana.SharedKernel.Domain;

namespace Jacana.PatientRegistration.Domain;

/// <summary>
/// Patient physical address. County is the minimum required component.
/// Uses the KenyaEMR address hierarchy: county → sub-county → ward,
/// with optional estate/village and landmark (the KenyaEMR "location" and
/// "landmark" fields) captured alongside the street line.
/// </summary>
public sealed class Address : ValueObject
{
    private Address(string county, string? subCounty, string? ward, string? line1, string? village, string? landmark)
    {
        County = county;
        SubCounty = subCounty;
        Ward = ward;
        Line1 = line1;
        Village = village;
        Landmark = landmark;
    }

    public string County { get; }
    public string? SubCounty { get; }
    public string? Ward { get; }
    public string? Line1 { get; }
    /// <summary>Estate / village (KenyaEMR location field).</summary>
    public string? Village { get; }
    /// <summary>Nearest landmark — used by community health teams for tracing.</summary>
    public string? Landmark { get; }

    public static Result<Address> Create(
        string county, string? subCounty = null, string? ward = null, string? line1 = null,
        string? village = null, string? landmark = null)
    {
        if (string.IsNullOrWhiteSpace(county))
            return Error.Validation("County is required.");
        return new Address(county.Trim(), NullIfEmpty(subCounty), NullIfEmpty(ward), NullIfEmpty(line1),
            NullIfEmpty(village), NullIfEmpty(landmark));
    }

    private static string? NullIfEmpty(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return County;
        yield return SubCounty;
        yield return Ward;
        yield return Line1;
        yield return Village;
        yield return Landmark;
    }
}
