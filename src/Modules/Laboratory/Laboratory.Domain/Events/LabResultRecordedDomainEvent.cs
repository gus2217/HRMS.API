using Jacana.SharedKernel.Domain;

namespace Jacana.Laboratory.Domain;

/// <summary>
/// Published when a lab test result is recorded. Handled outside the Laboratory module
/// (by Notifications) to alert the ordering clinician — never a direct cross-module call.
/// </summary>
public sealed record LabResultRecordedDomainEvent(
    Guid LabOrderId,
    Guid PatientId,
    Guid TestItemId,
    DateTime OccurredAtUtc) : IDomainEvent;
