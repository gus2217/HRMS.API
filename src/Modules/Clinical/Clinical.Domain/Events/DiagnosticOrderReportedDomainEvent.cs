using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Domain;

/// <summary>
/// Published when an imaging/procedure report is recorded — the ordering
/// clinician is alerted that the result is ready for review.
/// </summary>
public sealed record DiagnosticOrderReportedDomainEvent(
    Guid DiagnosticOrderId,
    Guid FacilityId,
    Guid PatientId,
    Guid? ConsultationId,
    Guid OrderedByUserId,
    DateTime OccurredAtUtc) : IDomainEvent;
