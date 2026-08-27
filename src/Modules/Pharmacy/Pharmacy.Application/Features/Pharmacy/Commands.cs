using Jacana.Pharmacy.Application.DTOs;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Domain;

namespace Jacana.Pharmacy.Application.Features.Pharmacy;

public sealed record CreatePrescriptionCommand(
    Guid PatientId,
    Guid ConsultationId,
    IReadOnlyList<PrescriptionItemInput> Items)
    : ICommand<Result<PrescriptionDetailDto>>;

public sealed record PrescriptionItemInput(Guid DrugId, string DosageInstructions, int QuantityPrescribed);

public sealed record DispenseMedicationCommand(
    Guid PrescriptionId,
    Guid PrescriptionItemId,
    int Quantity)
    : ICommand<Result<DispenseMedicationResponseDto>>;
