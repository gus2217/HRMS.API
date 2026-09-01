using Jacana.SharedKernel.Domain;

namespace Jacana.Laboratory.Domain;

/// <summary>
/// Published when a lab order completes (all tests resulted). Carries the
/// ordering clinician and patient so the Notifications module can alert the
/// doctor who ordered the tests.
/// </summary>
public sealed record LabOrderCompletedDomainEvent(
    Guid LabOrderId,
    Guid FacilityId,
    Guid PatientId,
    Guid ConsultationId,
    Guid OrderedByUserId,
    DateTime OccurredAtUtc) : IDomainEvent;
