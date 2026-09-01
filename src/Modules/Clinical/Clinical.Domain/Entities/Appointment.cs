using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Domain;

/// <summary>
/// A scheduled appointment. Recurring series are materialised as individual
/// occurrences that share a <see cref="RecurrenceGroupId"/> — each visit tracks
/// its own lifecycle, and calendar/day-queue queries stay simple and indexable.
/// </summary>
public sealed class Appointment : AggregateRoot<Guid>
{
    private Appointment() { } // EF

    private Appointment(
        Guid id, FacilityId facilityId, Guid patientId, string clinicType,
        AppointmentType type, DateTime scheduledAtUtc, int durationMinutes,
        string? reason, Guid? previousConsultationId, Guid? recurrenceGroupId,
        RecurrencePattern recurrencePattern, Guid createdByUserId, DateTime createdAtUtc)
        : base(id)
    {
        FacilityId = facilityId;
        PatientId = patientId;
        ClinicType = clinicType;
        Type = type;
        ScheduledAtUtc = scheduledAtUtc;
        DurationMinutes = durationMinutes;
        Reason = reason;
        PreviousConsultationId = previousConsultationId;
        RecurrenceGroupId = recurrenceGroupId;
        RecurrencePattern = recurrencePattern;
        CreatedByUserId = createdByUserId;
        CreatedAtUtc = createdAtUtc;
        Status = AppointmentStatus.Scheduled;
    }

    public FacilityId FacilityId { get; private set; } = null!;
    public Guid PatientId { get; private set; }
    public string ClinicType { get; private set; } = string.Empty;
    public AppointmentType Type { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public DateTime ScheduledAtUtc { get; private set; }
    public int DurationMinutes { get; private set; }
    public string? Reason { get; private set; }
    /// <summary>
    /// The prior consultation this follow-up/check-up continues. Carried into the
    /// consultation when the appointment is started so the visit stays in the
    /// same episode of care and prior diagnoses can be carried forward.
    /// </summary>
    public Guid? PreviousConsultationId { get; private set; }
    public Guid? RecurrenceGroupId { get; private set; }
    public RecurrencePattern RecurrencePattern { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public Guid? ConsultationId { get; private set; }
    public DateTime? StartedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }

    public static Result<Appointment> Create(
        Guid id, FacilityId facilityId, Guid patientId, string clinicType,
        AppointmentType type, DateTime scheduledAtUtc, int durationMinutes,
        string? reason, Guid? previousConsultationId, Guid? recurrenceGroupId,
        RecurrencePattern recurrencePattern, Guid createdByUserId, DateTime createdAtUtc)
    {
        if (patientId == Guid.Empty) return Error.Validation("Patient is required.");
        if (string.IsNullOrWhiteSpace(clinicType)) return Error.Validation("Clinic is required.");
        if (durationMinutes <= 0 || durationMinutes > 480)
            return Error.Validation("Duration must be between 1 and 480 minutes.");
        if (createdByUserId == Guid.Empty) return Error.Validation("Creator is required.");

        var appointment = new Appointment(id, facilityId, patientId, clinicType.Trim(), type,
            scheduledAtUtc, durationMinutes,
            string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
            previousConsultationId,
            recurrenceGroupId, recurrencePattern, createdByUserId, createdAtUtc);
        appointment.AddDomainEvent(new AppointmentRequestedDomainEvent(
            id, facilityId.Value, patientId, clinicType.Trim(), createdAtUtc));
        return appointment;
    }

    /// <summary>
    /// Starts the visit — links the freshly-registered consultation (Source=Appointment)
    /// and moves Scheduled → InProgress. Guarded: only a Scheduled appointment starts.
    /// </summary>
    public Result Start(Guid consultationId, DateTime startedAtUtc)
    {
        if (consultationId == Guid.Empty) return Error.Validation("Consultation is required.");
        if (Status != AppointmentStatus.Scheduled)
            return Error.InvalidOperation($"Cannot start an appointment in state {Status}.");

        Status = AppointmentStatus.InProgress;
        ConsultationId = consultationId;
        StartedAtUtc = startedAtUtc;
        return Result.Success();
    }

    /// <summary>Marks the appointment completed once its consultation completes.</summary>
    public Result Complete(DateTime completedAtUtc)
    {
        if (Status == AppointmentStatus.Completed) return Result.Success();
        if (Status != AppointmentStatus.InProgress)
            return Error.InvalidOperation($"Cannot complete an appointment in state {Status}.");

        Status = AppointmentStatus.Completed;
        CompletedAtUtc = completedAtUtc;
        return Result.Success();
    }

    public Result Cancel(DateTime cancelledAtUtc)
    {
        if (Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled)
            return Error.InvalidOperation($"Cannot cancel an appointment in state {Status}.");

        Status = AppointmentStatus.Cancelled;
        CompletedAtUtc = cancelledAtUtc;
        return Result.Success();
    }

    public Result MarkNoShow(DateTime atUtc)
    {
        if (Status is not (AppointmentStatus.Scheduled or AppointmentStatus.InProgress))
            return Error.InvalidOperation($"Cannot no-show an appointment in state {Status}.");

        Status = AppointmentStatus.NoShow;
        CompletedAtUtc = atUtc;
        return Result.Success();
    }
}
