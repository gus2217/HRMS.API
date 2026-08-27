using Jacana.Inpatient.Application.Abstractions;
using Jacana.Inpatient.Application.DTOs;
using Jacana.Inpatient.Domain;
using Jacana.Inpatient.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jacana.Inpatient.Infrastructure.Repositories;

public sealed class AdmissionRepository(InpatientDbContext db) : IAdmissionRepository
{
    public async Task<Admission?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Admissions.Include(a => a.Notes).FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task AddAsync(Admission admission, CancellationToken ct = default)
    {
        db.Admissions.Add(admission);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Admission admission, CancellationToken ct = default)
    {
        db.Entry(admission).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public async Task<AdmissionDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default)
    {
        var a = await db.Admissions.AsNoTracking()
            .Include(x => x.Notes)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (a is null) return null;

        return new AdmissionDetailDto(
            a.Id, a.PatientId, a.AdmittingClinicianUserId, a.WardName, a.BedNumber,
            a.Status.ToString(), a.AdmittedAtUtc, a.DischargedAtUtc,
            a.Notes.Select(n => new WardNoteDto(n.Content, n.AuthorUserId, n.RecordedAtUtc)).ToArray());
    }

    public async Task<IReadOnlyList<WardOccupancyDto>> GetWardOccupancyAsync(CancellationToken ct = default)
    {
        var grouped = await db.Admissions.AsNoTracking()
            .Where(a => a.Status != AdmissionStatus.Discharged)
            .GroupBy(a => a.WardName)
            .Select(g => new { WardName = g.Key, Occupied = g.Count() })
            .ToListAsync(ct);

        return grouped
            .Select(g => new WardOccupancyDto(g.WardName, g.Occupied, 0))
            .ToArray();
    }
}
