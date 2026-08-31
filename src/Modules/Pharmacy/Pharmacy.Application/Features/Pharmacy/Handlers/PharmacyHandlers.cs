using Jacana.Pharmacy.Application.Abstractions;
using Jacana.Pharmacy.Application.DTOs;
using Jacana.Pharmacy.Domain;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Pharmacy.Application.Features.Pharmacy.Handlers;

public sealed class CreatePrescriptionCommandHandler(
    IPrescriptionRepository prescriptions,
    Jacana.Inventory.Domain.IInventoryStockQuery stockQuery,
    Jacana.Inventory.Domain.IInventoryPricingQuery pricing,
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
            // Elite prescribe guard: a drug must exist in the catalog AND have
            // usable (non-expired) stock; the prescribed quantity may not exceed it.
            var price = await pricing.GetPriceAsync(item.DrugId, ct);
            if (price is null)
                return Error.InvalidOperation("The selected drug is not in the inventory catalog.");

            var available = await stockQuery.GetAvailableQuantityAsync(item.DrugId, ct);
            if (available <= 0)
                return Error.InvalidOperation($"{price.Name} is out of stock.");
            if (item.QuantityPrescribed > available)
                return Error.InvalidOperation(
                    $"Cannot prescribe {item.QuantityPrescribed} of {price.Name} — only {available} in stock.");

            var add = prescription.Value.AddItem(item.DrugId, item.DosageInstructions, item.QuantityPrescribed);
            if (add.IsFailure) return add.Error;
        }

        prescription.Value.PublishCreated();
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
    Jacana.Inventory.Domain.IInventoryStockService stockService,
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

        // Guard: never dispense more than the available stock — but only when stock
        // actually exists to protect. An untracked drug (no batches), or one whose
        // batches are all expired/zeroed, must not block dispensing entirely; the
        // prescribed-quantity rule in the domain still caps every dispense.
        var available = await stockQuery.GetAvailableQuantityAsync(item.DrugId, ct);
        if (available > 0 && request.Quantity > available)
            return Error.InvalidOperation($"Insufficient stock: only {available} available.");

        var dispense = prescription.DispenseItem(request.PrescriptionItemId, request.Quantity);
        if (dispense.IsFailure) return dispense.Error;

        var record = DispenseRecord.Create(
            Guid.NewGuid(), currentUser.FacilityId, request.PrescriptionItemId,
            request.Quantity, currentUser.UserId, clock.UtcNow);
        if (record.IsFailure) return record.Error;

        // Physically deduct the dispensed quantity from inventory (FEFO). Untracked
        // drugs with no usable batches have nothing to deduct — the deduction is a
        // no-op in that case (returns success with zero batches touched).
        var deduction = await stockService.DeductAsync(
            item.DrugId, request.Quantity, $"Rx-{prescription.Id}", currentUser.UserId, clock.UtcNow, ct);
        if (deduction.IsFailure) return deduction.Error;

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

public sealed class GetPrescriptionsByConsultationQueryHandler(IPrescriptionRepository prescriptions)
    : IRequestHandler<GetPrescriptionsByConsultationQuery, Result<IReadOnlyList<PrescriptionDetailDto>>>
{
    public async Task<Result<IReadOnlyList<PrescriptionDetailDto>>> Handle(
        GetPrescriptionsByConsultationQuery request, CancellationToken ct)
    {
        var items = await prescriptions.GetByConsultationAsync(request.ConsultationId, ct);
        return Result.Success(items);
    }
}

public sealed class SearchPrescriptionsQueryHandler(
    IPrescriptionRepository prescriptions,
    IPatientIdentityLookup patients)
    : IRequestHandler<SearchPrescriptionsQuery, Result<PagedResult<PrescriptionListItemDto>>>
{
    public async Task<Result<PagedResult<PrescriptionListItemDto>>> Handle(
        SearchPrescriptionsQuery request, CancellationToken ct)
    {
        var items = await prescriptions.SearchAsync(
            request.Status, request.PatientId, request.PageNumber, request.PageSize, ct);
        var total = await prescriptions.CountAsync(request.Status, request.PatientId, ct);

        var identities = await patients.GetIdentitiesAsync(
            items.Select(i => i.PatientId).ToArray(), ct);

        var rows = items.Select(p =>
        {
            identities.TryGetValue(p.PatientId, out var patient);
            return new PrescriptionListItemDto(
                p.Id, p.PatientId,
                patient?.PatientNumber ?? string.Empty,
                patient?.FullName ?? string.Empty,
                p.PrescribedByUserId, p.Status, p.PrescribedAtUtc, p.ItemCount);
        }).ToArray();

        return Result.Success(new PagedResult<PrescriptionListItemDto>(
            rows, total, request.PageNumber, request.PageSize));
    }
}
