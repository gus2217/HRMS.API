using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Domain;

/// <summary>
/// A referral from this facility to another unit/department or facility.
/// Recorded during the consultation so the clinician can escalate care
/// (e.g. general outpatient → orthopaedic clinic, or sub-county → county
/// referral hospital).
/// </summary>
public sealed class Referral : Entity<Guid>
{
    private Referral() { } // EF

    private Referral(
        Guid id,
        Guid consultationId,
        string referredToFacility,
        string? referredToUnit,
        string reason,
        ReferralPriority priority,
        string? notes,
        Guid referredByUserId,
        DateTime referredAtUtc)
        : base(id)
    {
        ConsultationId = consultationId;
        ReferredToFacility = referredToFacility;
        ReferredToUnit = referredToUnit;
        Reason = reason;
        Priority = priority;
        Notes = notes;
        ReferredByUserId = referredByUserId;
        ReferredAtUtc = referredAtUtc;
        Status = ReferralStatus.Pending;
    }

    public Guid ConsultationId { get; private set; }
    public string ReferredToFacility { get; private set; } = string.Empty;
    public string? ReferredToUnit { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public ReferralPriority Priority { get; private set; }
    public ReferralStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public Guid ReferredByUserId { get; private set; }
    public DateTime ReferredAtUtc { get; private set; }

    public static Result<Referral> Create(
        Guid consultationId,
        string referredToFacility,
        string? referredToUnit,
        string reason,
        ReferralPriority priority,
        string? notes,
        Guid referredByUserId,
        DateTime referredAtUtc)
    {
        if (string.IsNullOrWhiteSpace(referredToFacility))
            return Error.Validation("Referral destination is required.");
        if (string.IsNullOrWhiteSpace(reason))
            return Error.Validation("Referral reason is required.");
        if (!Enum.IsDefined(priority))
            return Error.Validation("Referral priority is invalid.");

        return new Referral(
            Guid.NewGuid(), consultationId, referredToFacility.Trim(), referredToUnit?.Trim(),
            reason.Trim(), priority, notes?.Trim(), referredByUserId, referredAtUtc);
    }
}
