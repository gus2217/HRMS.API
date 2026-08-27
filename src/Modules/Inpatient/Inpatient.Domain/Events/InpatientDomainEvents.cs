using Jacana.SharedKernel.Domain;

namespace Jacana.Inpatient.Domain;

/// <summary>Published when a patient is admitted. Consumed cross-module (e.g. Notifications).</summary>
public sealed record PatientAdmittedDomainEvent(
    Guid AdmissionId,
    Guid PatientId,
    string WardName,
    DateTime OccurredAtUtc) : IDomainEvent;

/// <summary>Published when a patient is discharged. Consumed cross-module (e.g. Notifications).</summary>
public sealed record PatientDischargedDomainEvent(
    Guid AdmissionId,
    Guid PatientId,
    DateTime OccurredAtUtc) : IDomainEvent;
