using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Domain;

/// <summary>Published when an appointment is booked (or a request raised) for a clinic.</summary>
public sealed record AppointmentRequestedDomainEvent(
    Guid AppointmentId,
    Guid FacilityId,
    Guid PatientId,
    string ClinicType,
    DateTime OccurredAtUtc) : IDomainEvent;
