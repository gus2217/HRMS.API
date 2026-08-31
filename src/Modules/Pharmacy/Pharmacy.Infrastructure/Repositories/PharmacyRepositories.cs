using Jacana.Pharmacy.Application.Abstractions;
using Jacana.Pharmacy.Application.DTOs;
using Jacana.Pharmacy.Domain;
using Jacana.Pharmacy.Infrastructure.Persistence;
using Jacana.Inventory.Domain;
using Microsoft.EntityFrameworkCore;
using Jacana.SharedKernel.Infrastructure.Persistence;

namespace Jacana.Pharmacy.Infrastructure.Repositories;

public sealed class PrescriptionRepository(PharmacyDbContext db, IInventoryPricingQuery pricing) : IPrescriptionRepository
{
    public async Task<Prescription?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Prescriptions.Include(p => p.Items).FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task AddAsync(Prescription prescription, CancellationToken ct = default)
    {
        db.Prescriptions.Add(prescription);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Prescription prescription, CancellationToken ct = default)
    {
        // Aggregate already tracked from GetByIdAsync. New children carry
        // client-generated keys; EF DetectChanges would classify them as Modified
        // (phantom UPDATE, 0 rows). Mark them Added explicitly while still Detached.
        db.MarkNewChildrenAdded(prescription);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Reserved = prescribed-but-not-yet-dispensed quantity across items that are
    /// still active (Pending / PartiallyDispensed / OutOfStock), joined to a
    /// prescription that isn't cancelled.
    /// </summary>
    private IQueryable<PrescriptionItem> ActiveItems()
        => db.PrescriptionItems
            .Where(i => i.Status != PrescriptionItemStatus.Dispensed
                && i.Status != PrescriptionItemStatus.Cancelled);

    public async Task<int> GetReservedQuantityAsync(Guid drugId, CancellationToken ct = default)
        => await ActiveItems()
            .Where(i => i.DrugId == drugId)
            .SumAsync(i => i.QuantityPrescribed - i.QuantityDispensed, ct);

    public async Task<IReadOnlyDictionary<Guid, int>> GetReservedQuantitiesAsync(CancellationToken ct = default)
    {
        var rows = await ActiveItems()
            .GroupBy(i => i.DrugId)
            .Select(g => new { DrugId = g.Key, Reserved = g.Sum(i => i.QuantityPrescribed - i.QuantityDispensed) })
            .ToListAsync(ct);
        return rows.ToDictionary(r => r.DrugId, r => r.Reserved);
    }

    public async Task<IReadOnlyList<PrescriptionSummaryDto>> SearchAsync(
        string? status, Guid? patientId, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = db.Prescriptions.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<PrescriptionStatus>(status, true, out var parsed))
            query = query.Where(p => p.Status == parsed);
        if (patientId.HasValue)
            query = query.Where(p => p.PatientId == patientId.Value);

        return await query
            .OrderByDescending(p => p.PrescribedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PrescriptionSummaryDto(
                p.Id, p.PatientId, p.PrescribedByUserId, p.Status.ToString(),
                p.PrescribedAtUtc, p.Items.Count))
            .ToListAsync(ct);
    }

    public Task<int> CountAsync(string? status, Guid? patientId, CancellationToken ct = default)
    {
        var query = db.Prescriptions.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<PrescriptionStatus>(status, true, out var parsed))
            query = query.Where(p => p.Status == parsed);
        if (patientId.HasValue)
            query = query.Where(p => p.PatientId == patientId.Value);
        return query.CountAsync(ct);
    }

    public async Task<PrescriptionDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default)
    {
        var p = await db.Prescriptions.AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (p is null) return null;

        var drugs = await pricing.GetPricesAsync(p.Items.Select(i => i.DrugId).Distinct().ToArray(), ct);

        return new PrescriptionDetailDto(
            p.Id, p.PatientId, p.ConsultationId, p.PrescribedByUserId,
            p.Status.ToString(), p.PrescribedAtUtc,
            p.Items.Select(i => ToItemDto(i, drugs)).ToArray());
    }

    public async Task<IReadOnlyList<PrescriptionDetailDto>> GetByConsultationAsync(Guid consultationId, CancellationToken ct = default)
    {
        var prescriptions = await db.Prescriptions.AsNoTracking()
            .Include(x => x.Items)
            .Where(x => x.ConsultationId == consultationId)
            .OrderByDescending(x => x.PrescribedAtUtc)
            .ToListAsync(ct);

        var drugIds = prescriptions.SelectMany(p => p.Items).Select(i => i.DrugId).Distinct().ToArray();
        var drugs = await pricing.GetPricesAsync(drugIds, ct);

        return prescriptions.Select(p => new PrescriptionDetailDto(
            p.Id, p.PatientId, p.ConsultationId, p.PrescribedByUserId,
            p.Status.ToString(), p.PrescribedAtUtc,
            p.Items.Select(i => ToItemDto(i, drugs)).ToArray())).ToArray();
    }

    private static PrescriptionItemDto ToItemDto(
        PrescriptionItem i, IReadOnlyDictionary<Guid, DrugPriceInfo> drugs)
    {
        drugs.TryGetValue(i.DrugId, out var drug);
        return new PrescriptionItemDto(
            i.Id, i.DrugId,
            drug?.Name ?? string.Empty, drug?.Category ?? string.Empty, drug?.Form ?? string.Empty,
            i.DosageInstructions, i.Route, i.Frequency, i.DurationDays,
            i.QuantityPrescribed, i.QuantityDispensed, i.Status.ToString());
    }
}

public sealed class DispenseRecordRepository(PharmacyDbContext db) : IDispenseRecordRepository
{
    public Task AddAsync(DispenseRecord record, CancellationToken ct = default)
    {
        db.DispenseRecords.Add(record);
        return Task.CompletedTask;
    }
}
