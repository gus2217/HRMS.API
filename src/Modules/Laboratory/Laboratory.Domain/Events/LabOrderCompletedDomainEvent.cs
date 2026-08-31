using Jacana.SharedKernel.Domain;

namespace Jacana.Laboratory.Domain;

/// <summary>
/// Published when a lab order's final test is resulted (status → Completed).
/// Consumed by Billing to charge the order's draft lines.
/// </summary>
public sealed record LabOrderCompletedDomainEvent(
    Guid LabOrderId,
    Guid ConsultationId,
    DateTime OccurredAtUtc) : IDomainEvent;
