using Jacana.Pharmacy.Application.Abstractions;
using Jacana.Pharmacy.Application.DTOs;
using Jacana.Pharmacy.Domain;
using Jacana.Pharmacy.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Jacana.SharedKernel.Infrastructure.Persistence;

namespace Jacana.Pharmacy.Infrastructure.Repositories;

public sealed class PrescriptionRepository(PharmacyDbContext db) : IPrescriptionRepository
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

    public async Task<PrescriptionDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default)
    {
        var p = await db.Prescriptions.AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (p is null) return null;

        return new PrescriptionDetailDto(
            p.Id, p.PatientId, p.ConsultationId, p.PrescribedByUserId,
            p.Status.ToString(), p.PrescribedAtUtc,
            p.Items.Select(i => new PrescriptionItemDto(
                i.Id, i.DrugId, i.DosageInstructions, i.QuantityPrescribed,
                i.QuantityDispensed, i.Status.ToString())).ToArray());
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
