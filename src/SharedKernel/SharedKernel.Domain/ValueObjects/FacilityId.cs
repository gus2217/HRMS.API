namespace Jacana.SharedKernel.Domain;

/// <summary>
/// Tenant partition key. Present on every aggregate so the system is multi-tenant
/// from day one even though the first deployment is a single facility.
/// </summary>
public sealed class FacilityId : ValueObject
{
    private FacilityId(Guid value) => Value = value;

    public Guid Value { get; }

    public static FacilityId New() => new(Guid.NewGuid());
    public static FacilityId From(Guid value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
