using Jacana.SharedKernel.Domain;

namespace Jacana.Laboratory.Domain;

/// <summary>
/// Published when a lab order is created. Consumed by Billing to auto-bill the
/// consultation's test lines — never a direct cross-module call.
/// </summary>
public sealed record LabOrderCreatedDomainEvent(
    Guid LabOrderId,
    Guid FacilityId,
    Guid PatientId,
    Guid ConsultationId,
    IReadOnlyList<LabTestData> Tests,
    DateTime OccurredAtUtc) : IDomainEvent;

public sealed record LabTestData(string TestCode, string TestName);
