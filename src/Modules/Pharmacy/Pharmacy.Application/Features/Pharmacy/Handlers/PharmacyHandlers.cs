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
        // Only enforced when the drug is actually stock-tracked — an untracked drug
        // (no batches ever received) must not block dispensing entirely.
        var isTracked = await stockQuery.IsTrackedAsync(item.DrugId, ct);
        if (isTracked)
        {
            var available = await stockQuery.GetAvailableQuantityAsync(item.DrugId, ct);
            if (request.Quantity > available)
                return Error.InvalidOperation($"Insufficient stock: only {available} available.");
        }

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
