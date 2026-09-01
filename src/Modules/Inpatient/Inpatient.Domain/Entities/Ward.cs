using Jacana.SharedKernel.Domain;

namespace Jacana.Inpatient.Domain;

/// <summary>
/// A ward/bay that patients are admitted into. Wards are created by the
/// administrator (name, type, total beds) and referenced by admissions; the
/// occupancy dashboard groups active admissions by ward.
/// </summary>
public sealed class Ward : AggregateRoot<Guid>
{
    private Ward() { } // EF

    private Ward(Guid id, FacilityId facilityId, string name, WardType type, int totalBeds)
        : base(id)
    {
        FacilityId = facilityId;
        Name = name;
        Type = type;
        TotalBeds = totalBeds;
        IsActive = true;
    }

    public FacilityId FacilityId { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public WardType Type { get; private set; }
    public int TotalBeds { get; private set; }
    public bool IsActive { get; private set; }

    public static Result<Ward> Create(
        FacilityId facilityId, string name, WardType type, int totalBeds)
    {
        if (string.IsNullOrWhiteSpace(name)) return Error.Validation("Ward name is required.");
        if (totalBeds <= 0) return Error.Validation("Total beds must be greater than zero.");
        return new Ward(Guid.NewGuid(), facilityId, name.Trim(), type, totalBeds);
    }

    public Result Update(string name, WardType type, int totalBeds)
    {
        if (string.IsNullOrWhiteSpace(name)) return Error.Validation("Ward name is required.");
        if (totalBeds <= 0) return Error.Validation("Total beds must be greater than zero.");
        Name = name.Trim();
        Type = type;
        TotalBeds = totalBeds;
        return Result.Success();
    }

    public void Deactivate() => IsActive = false;
    public void Reactivate() => IsActive = true;
}
