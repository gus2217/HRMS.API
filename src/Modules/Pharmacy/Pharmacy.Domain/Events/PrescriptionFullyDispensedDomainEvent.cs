using Jacana.SharedKernel.Domain;

namespace Jacana.Pharmacy.Domain;

/// <summary>
/// Published when a prescription's final item is dispensed (status → FullyDispensed).
/// Consumed by Billing to charge the prescription's draft lines.
/// </summary>
public sealed record PrescriptionFullyDispensedDomainEvent(
    Guid PrescriptionId,
    Guid ConsultationId,
    DateTime OccurredAtUtc) : IDomainEvent;
