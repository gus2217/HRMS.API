using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Domain;

/// <summary>
/// A diagnostic order — imaging (X-ray, ultrasound…) or a procedure (minor
/// surgery, wound care…). Tied to a consultation, tracked through a
/// Ordered → Scheduled → Performed → Reported lifecycle, and carries the
/// clinician's report when finalised. Mirrors the OpenMRS "Imaging Orders" and
/// "Procedure Orders" widgets as one unified order type.
/// </summary>
public sealed class DiagnosticOrder : AggregateRoot<Guid>
{
    private DiagnosticOrder() { } // EF

    private DiagnosticOrder(
        Guid id,
        FacilityId facilityId,
        Guid patientId,
        Guid? consultationId,
        DiagnosticOrderType type,
        string name,
        string? bodySite,
        string clinicalIndication,
        DiagnosticOrderPriority priority,
        Guid orderedByUserId,
        DateTime orderedAtUtc)
        : base(id)
    {
        FacilityId = facilityId;
        PatientId = patientId;
        ConsultationId = consultationId;
        Type = type;
        Name = name;
        BodySite = bodySite;
        ClinicalIndication = clinicalIndication;
        Priority = priority;
        Status = DiagnosticOrderStatus.Ordered;
        OrderedByUserId = orderedByUserId;
        OrderedAtUtc = orderedAtUtc;
    }

    public FacilityId FacilityId { get; private set; } = null!;
    public Guid PatientId { get; private set; }
    public Guid? ConsultationId { get; private set; }
    public DiagnosticOrderType Type { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? BodySite { get; private set; }
    public string ClinicalIndication { get; private set; } = string.Empty;
    public DiagnosticOrderPriority Priority { get; private set; }
    public DiagnosticOrderStatus Status { get; private set; }
    public Guid OrderedByUserId { get; private set; }
    public DateTime OrderedAtUtc { get; private set; }
    public Guid? ScheduledByUserId { get; private set; }
    public DateTime? ScheduledAtUtc { get; private set; }
    public Guid? PerformedByUserId { get; private set; }
    public DateTime? PerformedAtUtc { get; private set; }
    public string? Report { get; private set; }
    public Guid? ReportedByUserId { get; private set; }
    public DateTime? ReportedAtUtc { get; private set; }
    public Guid? CancelledByUserId { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public string? CancellationReason { get; private set; }

    public static Result<DiagnosticOrder> Create(
        FacilityId facilityId,
        Guid patientId,
        Guid? consultationId,
        DiagnosticOrderType type,
        string name,
        string? bodySite,
        string clinicalIndication,
        DiagnosticOrderPriority priority,
        Guid orderedByUserId,
        DateTime orderedAtUtc)
    {
        if (patientId == Guid.Empty) return Error.Validation("Patient is required.");
        if (orderedByUserId == Guid.Empty) return Error.Validation("Ordering user is required.");
        if (string.IsNullOrWhiteSpace(name)) return Error.Validation("Order name is required.");
        if (string.IsNullOrWhiteSpace(clinicalIndication)) return Error.Validation("Clinical indication is required.");

        return new DiagnosticOrder(
            Guid.NewGuid(), facilityId, patientId, consultationId, type, name.Trim(),
            bodySite?.Trim(), clinicalIndication.Trim(), priority, orderedByUserId, orderedAtUtc);
    }

    public Result Schedule(Guid scheduledByUserId, DateTime scheduledAtUtc)
    {
        if (Status != DiagnosticOrderStatus.Ordered)
            return Error.InvalidOperation($"Cannot schedule an order in status {Status}.");

        ScheduledByUserId = scheduledByUserId;
        ScheduledAtUtc = scheduledAtUtc;
        Status = DiagnosticOrderStatus.Scheduled;
        return Result.Success();
    }

    public Result MarkPerformed(Guid performedByUserId, DateTime performedAtUtc)
    {
        if (Status is not (DiagnosticOrderStatus.Ordered or DiagnosticOrderStatus.Scheduled))
            return Error.InvalidOperation($"Cannot perform an order in status {Status}.");

        PerformedByUserId = performedByUserId;
        PerformedAtUtc = performedAtUtc;
        Status = DiagnosticOrderStatus.Performed;
        return Result.Success();
    }

    public Result RecordReport(string report, Guid reportedByUserId, DateTime reportedAtUtc)
    {
        if (Status != DiagnosticOrderStatus.Performed)
            return Error.InvalidOperation($"Cannot report an order in status {Status}.");
        if (string.IsNullOrWhiteSpace(report)) return Error.Validation("Report is required.");

        Report = report.Trim();
        ReportedByUserId = reportedByUserId;
        ReportedAtUtc = reportedAtUtc;
        Status = DiagnosticOrderStatus.Reported;

        AddDomainEvent(new DiagnosticOrderReportedDomainEvent(
            Id, FacilityId.Value, PatientId, ConsultationId, OrderedByUserId, reportedAtUtc));
        return Result.Success();
    }

    public Result Cancel(string reason, Guid cancelledByUserId, DateTime cancelledAtUtc)
    {
        if (Status is DiagnosticOrderStatus.Reported or DiagnosticOrderStatus.Cancelled)
            return Error.InvalidOperation($"Cannot cancel an order in status {Status}.");
        if (string.IsNullOrWhiteSpace(reason))
            return Error.Validation("A cancellation reason is required.");

        CancellationReason = reason.Trim();
        CancelledByUserId = cancelledByUserId;
        CancelledAtUtc = cancelledAtUtc;
        Status = DiagnosticOrderStatus.Cancelled;
        return Result.Success();
    }
}
