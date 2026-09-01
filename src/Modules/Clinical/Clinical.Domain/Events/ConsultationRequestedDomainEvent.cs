using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Domain;

/// <summary>Published when a patient is queued for a consultation at a clinic.</summary>
public sealed record ConsultationRequestedDomainEvent(
    Guid QueueEntryId,
    Guid FacilityId,
    Guid PatientId,
    string ClinicType,
    Guid RequestedByUserId,
    DateTime OccurredAtUtc) : IDomainEvent;
