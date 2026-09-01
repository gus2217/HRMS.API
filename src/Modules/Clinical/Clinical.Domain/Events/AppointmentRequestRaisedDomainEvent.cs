using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Domain;

/// <summary>Published when reception raises an appointment request for a clinic.</summary>
public sealed record AppointmentRequestRaisedDomainEvent(
    Guid AppointmentRequestId,
    Guid FacilityId,
    Guid PatientId,
    string ClinicType,
    DateTime OccurredAtUtc) : IDomainEvent;
