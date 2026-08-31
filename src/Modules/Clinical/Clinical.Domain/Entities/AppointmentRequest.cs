using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Domain;

/// <summary>
/// A reception-raised request for an appointment, targeted at a specific clinic
/// and awaiting a clinician's approval. On approval the approver schedules a real
/// <see cref="Appointment"/> which is linked back here for traceability.
/// </summary>
public sealed class AppointmentRequest : AggregateRoot<Guid>
{
    private AppointmentRequest() { } // EF

    private AppointmentRequest(
        Guid id, FacilityId facilityId, Guid patientId, string clinicType,
        string reason, string? notes, DateOnly? preferredDate, Guid requestedByUserId, DateTime requestedAtUtc)
        : base(id)
    {
        FacilityId = facilityId;
        PatientId = patientId;
        ClinicType = clinicType;
        Reason = reason;
        Notes = notes;
        PreferredDate = preferredDate;
        RequestedByUserId = requestedByUserId;
        RequestedAtUtc = requestedAtUtc;
        Status = AppointmentRequestStatus.Pending;
    }

    public FacilityId FacilityId { get; private set; } = null!;
    public Guid PatientId { get; private set; }
    public string ClinicType { get; private set; } = string.Empty;
    public string Reason { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public DateOnly? PreferredDate { get; private set; }
    public AppointmentRequestStatus Status { get; private set; }
    public Guid RequestedByUserId { get; private set; }
    public DateTime RequestedAtUtc { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }
    public DateTime? ApprovedAtUtc { get; private set; }
    public Guid? AppointmentId { get; private set; }

    public static Result<AppointmentRequest> Create(
        Guid id, FacilityId facilityId, Guid patientId, string clinicType,
        string reason, string? notes, DateOnly? preferredDate, Guid requestedByUserId, DateTime requestedAtUtc)
    {
        if (patientId == Guid.Empty) return Error.Validation("Patient is required.");
        if (string.IsNullOrWhiteSpace(clinicType)) return Error.Validation("Clinic is required.");
        if (string.IsNullOrWhiteSpace(reason)) return Error.Validation("Reason is required.");
        if (requestedByUserId == Guid.Empty) return Error.Validation("Requester is required.");

        return new AppointmentRequest(id, facilityId, patientId, clinicType.Trim(),
            reason.Trim(), string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            preferredDate, requestedByUserId, requestedAtUtc);
    }

    /// <summary>Clinician approves and schedules the appointment.</summary>
    public Result Approve(Guid approvedByUserId, Guid appointmentId, DateTime approvedAtUtc)
    {
        if (Status != AppointmentRequestStatus.Pending)
            return Error.InvalidOperation($"Cannot approve a request in state {Status}.");
        if (appointmentId == Guid.Empty) return Error.Validation("Appointment is required.");

        Status = AppointmentRequestStatus.Approved;
        ApprovedByUserId = approvedByUserId;
        ApprovedAtUtc = approvedAtUtc;
        AppointmentId = appointmentId;
        return Result.Success();
    }

    public Result Decline(Guid declinedByUserId, DateTime declinedAtUtc)
    {
        if (Status != AppointmentRequestStatus.Pending)
            return Error.InvalidOperation($"Cannot decline a request in state {Status}.");

        Status = AppointmentRequestStatus.Declined;
        ApprovedByUserId = declinedByUserId;
        ApprovedAtUtc = declinedAtUtc;
        return Result.Success();
    }
}
