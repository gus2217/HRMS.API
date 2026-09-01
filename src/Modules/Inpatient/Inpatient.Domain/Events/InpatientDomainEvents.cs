using Jacana.SharedKernel.Domain;

namespace Jacana.Inpatient.Domain;

/// <summary>Published when a patient is admitted. Consumed cross-module (e.g. Notifications).</summary>
public sealed record PatientAdmittedDomainEvent(
    Guid AdmissionId,
    Guid FacilityId,
    Guid PatientId,
    string WardName,
    DateTime OccurredAtUtc) : IDomainEvent;

/// <summary>Published when a patient is discharged. Consumed cross-module (e.g. Notifications).</summary>
public sealed record PatientDischargedDomainEvent(
    Guid AdmissionId,
    Guid FacilityId,
    Guid PatientId,
    DateTime OccurredAtUtc) : IDomainEvent;

/// <summary>Published when a patient is transferred between wards. Consumed cross-module (e.g. Notifications).</summary>
public sealed record PatientTransferredDomainEvent(
    Guid AdmissionId,
    Guid FacilityId,
    Guid PatientId,
    Guid FromWardId,
    string FromWardName,
    Guid ToWardId,
    string ToWardName,
    DateTime OccurredAtUtc) : IDomainEvent;
