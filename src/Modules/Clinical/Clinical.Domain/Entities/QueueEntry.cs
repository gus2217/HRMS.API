using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Domain;

/// <summary>
/// A consultation queue entry — created by reception, allocated to a clinic,
/// accepted by a clinician. Acceptance atomically registers a <see cref="Consultation"/>
/// (same aggregate owner, same transaction), so the patient's visit flows
/// queue → consultation → completion without re-entry.
/// </summary>
public sealed class QueueEntry : AggregateRoot<Guid>
{
    private QueueEntry() { } // EF

    private QueueEntry(
        Guid id, FacilityId facilityId, Guid patientId, string clinicType,
        QueuePriority priority, string? notes, string queueNumber,
        Guid requestedByUserId, DateTime requestedAtUtc)
        : base(id)
    {
        FacilityId = facilityId;
        PatientId = patientId;
        ClinicType = clinicType;
        Priority = priority;
        Notes = notes;
        QueueNumber = queueNumber;
        RequestedByUserId = requestedByUserId;
        RequestedAtUtc = requestedAtUtc;
        Status = QueueStatus.Waiting;
    }

    public FacilityId FacilityId { get; private set; } = null!;
    public Guid PatientId { get; private set; }
    public string ClinicType { get; private set; } = string.Empty;
    public QueuePriority Priority { get; private set; }
    public QueueStatus Status { get; private set; }
    public string QueueNumber { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public Guid RequestedByUserId { get; private set; }
    public DateTime RequestedAtUtc { get; private set; }
    public Guid? AcceptedByUserId { get; private set; }
    public DateTime? AcceptedAtUtc { get; private set; }
    public Guid? ConsultationId { get; private set; }

    public static Result<QueueEntry> Create(
        Guid id, FacilityId facilityId, Guid patientId, string clinicType,
        QueuePriority priority, string? notes, string queueNumber,
        Guid requestedByUserId, DateTime requestedAtUtc)
    {
        if (patientId == Guid.Empty) return Error.Validation("Patient is required.");
        if (string.IsNullOrWhiteSpace(clinicType)) return Error.Validation("Clinic is required.");
        if (requestedByUserId == Guid.Empty) return Error.Validation("Requester is required.");

        var entry = new QueueEntry(id, facilityId, patientId, clinicType.Trim(),
            priority, string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            queueNumber, requestedByUserId, requestedAtUtc);
        entry.AddDomainEvent(new ConsultationRequestedDomainEvent(
            id, facilityId.Value, patientId, clinicType.Trim(), requestedByUserId, requestedAtUtc));
        return entry;
    }

    /// <summary>
    /// Accepts the queue entry and links the freshly-registered consultation.
    /// Guarded: only Waiting entries can be accepted, and only once.
    /// </summary>
    public Result Accept(Guid clinicianUserId, Guid consultationId, DateTime acceptedAtUtc)
    {
        if (clinicianUserId == Guid.Empty) return Error.Validation("Clinician is required.");
        if (Status != QueueStatus.Waiting)
            return Error.InvalidOperation($"Cannot accept a queue entry in state {Status}.");
        if (consultationId == Guid.Empty) return Error.Validation("Consultation is required.");

        Status = QueueStatus.Accepted;
        AcceptedByUserId = clinicianUserId;
        AcceptedAtUtc = acceptedAtUtc;
        ConsultationId = consultationId;
        return Result.Success();
    }

    /// <summary>Marks the entry completed when its linked consultation completes.</summary>
    public Result Complete(DateTime completedAtUtc)
    {
        if (Status is not (QueueStatus.Waiting or QueueStatus.Accepted))
            return Error.InvalidOperation($"Cannot complete a queue entry in state {Status}.");
        if (Status == QueueStatus.Waiting)
            return Error.InvalidOperation("Cannot complete a queue entry that has not been accepted.");

        Status = QueueStatus.Completed;
        return Result.Success();
    }

    /// <summary>Reception cancels a waiting entry (patient left, duplicate, walk-in no-show).</summary>
    public Result Cancel(Guid cancelledByUserId, DateTime cancelledAtUtc)
    {
        if (Status != QueueStatus.Waiting)
            return Error.InvalidOperation($"Cannot cancel a queue entry in state {Status}.");
        Status = QueueStatus.Cancelled;
        return Result.Success();
    }
}
