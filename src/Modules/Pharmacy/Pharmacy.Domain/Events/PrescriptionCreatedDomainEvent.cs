using Jacana.SharedKernel.Domain;

namespace Jacana.Pharmacy.Domain;

/// <summary>
/// Published when a prescription is created. Consumed by Billing to auto-bill the
/// consultation's medication lines — never a direct cross-module call.
/// </summary>
public sealed record PrescriptionCreatedDomainEvent(
    Guid PrescriptionId,
    Guid FacilityId,
    Guid PatientId,
    Guid ConsultationId,
    IReadOnlyList<PrescriptionItemData> Items,
    DateTime OccurredAtUtc) : IDomainEvent;

public sealed record PrescriptionItemData(
    Guid DrugId,
    string DosageInstructions,
    string Route,
    string Frequency,
    int? DurationDays,
    int QuantityPrescribed);
