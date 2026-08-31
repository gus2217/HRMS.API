namespace Jacana.Pharmacy.Application.DTOs;

// HTTP request bindings for pharmacy endpoints.

public sealed record CreatePrescriptionRequestDto(
    Guid PatientId,
    Guid ConsultationId,
    IReadOnlyList<PrescriptionItemRequestDto> Items);

public sealed record PrescriptionItemRequestDto(
    Guid DrugId,
    string DosageInstructions,
    string Route,
    string Frequency,
    int? DurationDays,
    int QuantityPrescribed);

public sealed record DispenseMedicationRequestDto(Guid PrescriptionId, Guid PrescriptionItemId, int Quantity);
