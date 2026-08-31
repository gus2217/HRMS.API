using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Domain;

/// <summary>
/// Published when a consultation enters active clinical work (→ InConsultation).
/// Consumed by Billing to open the patient's draft invoice and accrue the
/// consultation fee, so the doctor sees a running bill the moment the visit opens.
/// </summary>
public sealed record ConsultationStartedDomainEvent(
    Guid ConsultationId,
    Guid FacilityId,
    Guid PatientId,
    DateTime OccurredAtUtc) : IDomainEvent;
