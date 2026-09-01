using Jacana.Inpatient.Application.Abstractions;
using Jacana.Inpatient.Application.DTOs;
using Jacana.Inpatient.Domain;
using Jacana.Inpatient.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jacana.Inpatient.Infrastructure.Repositories;

public sealed class WardRepository(InpatientDbContext db) : IWardRepository
{
    public Task<Ward?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Wards.FirstOrDefaultAsync(w => w.Id == id, ct);

    public Task AddAsync(Ward ward, CancellationToken ct = default)
    {
        db.Wards.Add(ward);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Ward ward, CancellationToken ct = default)
        => Task.CompletedTask; // already tracked from GetByIdAsync

    public async Task<IReadOnlyList<WardDto>> ListAsync(bool activeOnly, CancellationToken ct = default)
    {
        var query = db.Wards.AsNoTracking();
        if (activeOnly)
            query = query.Where(w => w.IsActive);

        return await query
            .OrderBy(w => w.Name)
            .Select(w => new WardDto(
                w.Id, w.Name, w.Type.ToString(), w.TotalBeds, w.IsActive))
            .ToListAsync(ct);
    }
}
