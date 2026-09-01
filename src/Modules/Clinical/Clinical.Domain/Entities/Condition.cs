using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Domain;

/// <summary>
/// A persistent condition on the patient's problem list. Unlike a consultation
/// <see cref="Diagnosis"/> (which is scoped to one visit), a condition persists
/// across visits and carries a status (active / resolved / inactive) so chronic
/// conditions are tracked longitudinally. Mirrors the OpenMRS "Conditions" widget.
/// </summary>
public sealed class Condition : AggregateRoot<Guid>
{
    private Condition() { } // EF

    private Condition(
        Guid id,
        FacilityId facilityId,
        Guid patientId,
        string? code,
        string description,
        DateTime onsetDate,
        Guid recordedByUserId,
        DateTime recordedAtUtc)
        : base(id)
    {
        FacilityId = facilityId;
        PatientId = patientId;
        Code = code;
        Description = description;
        Status = ConditionStatus.Active;
        OnsetDate = onsetDate;
        RecordedByUserId = recordedByUserId;
        RecordedAtUtc = recordedAtUtc;
    }

    public FacilityId FacilityId { get; private set; } = null!;
    public Guid PatientId { get; private set; }
    public string? Code { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public ConditionStatus Status { get; private set; }
    public DateTime OnsetDate { get; private set; }
    public DateTime? ResolvedDate { get; private set; }
    public Guid RecordedByUserId { get; private set; }
    public DateTime RecordedAtUtc { get; private set; }

    public static Result<Condition> Add(
        FacilityId facilityId,
        Guid patientId,
        string? code,
        string description,
        DateTime onsetDate,
        Guid recordedByUserId,
        DateTime recordedAtUtc)
    {
        if (patientId == Guid.Empty) return Error.Validation("Patient is required.");
        if (recordedByUserId == Guid.Empty) return Error.Validation("Recorder is required.");
        if (string.IsNullOrWhiteSpace(description)) return Error.Validation("Condition description is required.");

        return new Condition(
            Guid.NewGuid(), facilityId, patientId,
            code?.Trim(), description.Trim(), onsetDate, recordedByUserId, recordedAtUtc);
    }

    public Result Resolve(DateTime resolvedDate)
    {
        if (Status == ConditionStatus.Resolved)
            return Error.InvalidOperation("Condition is already resolved.");
        if (resolvedDate < OnsetDate.Date)
            return Error.Validation("Resolution date cannot be before the onset date.");

        Status = ConditionStatus.Resolved;
        ResolvedDate = resolvedDate;
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (Status == ConditionStatus.Resolved)
            return Error.InvalidOperation("A resolved condition cannot be deactivated.");
        Status = ConditionStatus.Inactive;
        return Result.Success();
    }
}
