using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Domain;

/// <summary>
/// A sticky alert on a patient's record — an allergy warning, a safety note
/// (fall risk, NPO, isolation) or an informational flag. Mirrors the OpenMRS
/// "Patient Flags" widget. Flags surface prominently on the patient banner.
/// </summary>
public sealed class PatientFlag : AggregateRoot<Guid>
{
    private PatientFlag() { } // EF

    private PatientFlag(
        Guid id,
        FacilityId facilityId,
        Guid patientId,
        PatientFlagType type,
        string message,
        Guid createdByUserId,
        DateTime createdAtUtc)
        : base(id)
    {
        FacilityId = facilityId;
        PatientId = patientId;
        Type = type;
        Message = message;
        IsActive = true;
        CreatedByUserId = createdByUserId;
        CreatedAtUtc = createdAtUtc;
    }

    public FacilityId FacilityId { get; private set; } = null!;
    public Guid PatientId { get; private set; }
    public PatientFlagType Type { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public Guid? DeactivatedByUserId { get; private set; }
    public DateTime? DeactivatedAtUtc { get; private set; }

    public static Result<PatientFlag> Raise(
        FacilityId facilityId,
        Guid patientId,
        PatientFlagType type,
        string message,
        Guid createdByUserId,
        DateTime createdAtUtc)
    {
        if (patientId == Guid.Empty) return Error.Validation("Patient is required.");
        if (createdByUserId == Guid.Empty) return Error.Validation("Creator is required.");
        if (string.IsNullOrWhiteSpace(message)) return Error.Validation("Flag message is required.");

        return new PatientFlag(
            Guid.NewGuid(), facilityId, patientId, type, message.Trim(), createdByUserId, createdAtUtc);
    }

    public Result Deactivate(Guid deactivatedByUserId, DateTime deactivatedAtUtc)
    {
        if (!IsActive) return Error.InvalidOperation("Flag is already inactive.");
        IsActive = false;
        DeactivatedByUserId = deactivatedByUserId;
        DeactivatedAtUtc = deactivatedAtUtc;
        return Result.Success();
    }
}
