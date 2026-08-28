namespace Jacana.Pharmacy.Application.DTOs;

public sealed record PrescriptionItemDto(
    Guid Id,
    Guid DrugId,
    string DosageInstructions,
    int QuantityPrescribed,
    int QuantityDispensed,
    string Status);

public sealed record PrescriptionSummaryDto(
    Guid Id,
    Guid PatientId,
    Guid PrescribedByUserId,
    string Status,
    DateTime PrescribedAtUtc,
    int ItemCount);

/// <summary>List-view row with patient display identity resolved cross-schema.</summary>
public sealed record PrescriptionListItemDto(
    Guid Id,
    Guid PatientId,
    string PatientNumber,
    string PatientName,
    Guid PrescribedByUserId,
    string Status,
    DateTime PrescribedAtUtc,
    int ItemCount);

public sealed record PrescriptionDetailDto(
    Guid Id,
    Guid PatientId,
    Guid ConsultationId,
    Guid PrescribedByUserId,
    string Status,
    DateTime PrescribedAtUtc,
    IReadOnlyList<PrescriptionItemDto> Items);

public sealed record DispenseMedicationResponseDto(
    Guid DispenseRecordId,
    Guid PrescriptionItemId,
    int QuantityDispensed);
