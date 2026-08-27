using Jacana.Pharmacy.Application.Abstractions;
using Jacana.Pharmacy.Application.DTOs;
using Jacana.Pharmacy.Domain;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Pharmacy.Application.Features.Pharmacy.Handlers;

public sealed class CreatePrescriptionCommandHandler(
    IPrescriptionRepository prescriptions,
    ICurrentUser currentUser,
    IClock clock)
    : IRequestHandler<CreatePrescriptionCommand, Result<PrescriptionDetailDto>>
{
    public async Task<Result<PrescriptionDetailDto>> Handle(CreatePrescriptionCommand request, CancellationToken ct)
    {
        var prescription = Prescription.Create(
            Guid.NewGuid(), currentUser.FacilityId, request.PatientId, request.ConsultationId,
            currentUser.UserId, clock.UtcNow);
        if (prescription.IsFailure) return prescription.Error;

        foreach (var item in request.Items)
        {
            var add = prescription.Value.AddItem(item.DrugId, item.DosageInstructions, item.QuantityPrescribed);
            if (add.IsFailure) return add.Error;
        }

        await prescriptions.AddAsync(prescription.Value, ct);
        // Map from the in-memory aggregate — the unit-of-work transaction has not
        // committed yet, so a re-query would not see the new row.
        return PrescriptionMapper.ToDetail(prescription.Value);
    }
}

public sealed class DispenseMedicationCommandHandler(
    IPrescriptionRepository prescriptions,
    IDispenseRecordRepository dispenseRecords,
    Jacana.Inventory.Domain.IInventoryStockQuery stockQuery,
    ICurrentUser currentUser,
    IClock clock)
    : IRequestHandler<DispenseMedicationCommand, Result<DispenseMedicationResponseDto>>
{
    public async Task<Result<DispenseMedicationResponseDto>> Handle(DispenseMedicationCommand request, CancellationToken ct)
    {
        var prescription = await prescriptions.GetByIdAsync(request.PrescriptionId, ct);
        if (prescription is null) return Error.NotFound("Prescription not found.");

        var item = prescription.Items.FirstOrDefault(i => i.Id == request.PrescriptionItemId);
        if (item is null) return Error.NotFound("Prescription item not found.");

        // Guard: cannot dispense more than available stock (via Inventory read contract).
        var available = await stockQuery.GetAvailableQuantityAsync(item.DrugId, ct);
        if (request.Quantity > available)
            return Error.InvalidOperation($"Insufficient stock: only {available} available.");

        var dispense = prescription.DispenseItem(request.PrescriptionItemId, request.Quantity);
        if (dispense.IsFailure) return dispense.Error;

        var record = DispenseRecord.Create(
            Guid.NewGuid(), currentUser.FacilityId, request.PrescriptionItemId,
            request.Quantity, currentUser.UserId, clock.UtcNow);
        if (record.IsFailure) return record.Error;

        await prescriptions.UpdateAsync(prescription, ct);
        await dispenseRecords.AddAsync(record.Value, ct);

        return new DispenseMedicationResponseDto(record.Value.Id, request.PrescriptionItemId, request.Quantity);
    }
}

public sealed class GetPrescriptionQueryHandler(IPrescriptionRepository prescriptions)
    : IRequestHandler<GetPrescriptionQuery, Result<PrescriptionDetailDto>>
{
    public async Task<Result<PrescriptionDetailDto>> Handle(GetPrescriptionQuery request, CancellationToken ct)
    {
        var detail = await prescriptions.GetDetailAsync(request.PrescriptionId, ct);
        return detail is null ? Error.NotFound("Prescription not found.") : detail;
    }
}
