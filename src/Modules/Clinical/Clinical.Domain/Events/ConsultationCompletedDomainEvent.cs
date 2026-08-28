using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Domain;

/// <summary>
/// Published when a consultation is completed (patient departs). Consumed by Billing
/// to add the consultation fee and issue the accumulated invoice.
/// </summary>
public sealed record ConsultationCompletedDomainEvent(
    Guid ConsultationId,
    Guid FacilityId,
    Guid PatientId,
    DateTime OccurredAtUtc) : IDomainEvent;
