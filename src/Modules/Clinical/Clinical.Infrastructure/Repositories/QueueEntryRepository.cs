using Jacana.Clinical.Application.Abstractions;
using Jacana.Clinical.Application.DTOs;
using Jacana.Clinical.Domain;
using Jacana.Clinical.Infrastructure.Persistence;
using Jacana.SharedKernel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jacana.Clinical.Infrastructure.Repositories;

public sealed class QueueEntryRepository(ClinicalDbContext db) : IQueueEntryRepository
{
    public Task<QueueEntry?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.QueueEntries.FirstOrDefaultAsync(q => q.Id == id, ct);

    public Task<QueueEntry?> GetByConsultationIdAsync(Guid consultationId, CancellationToken ct = default)
        => db.QueueEntries.FirstOrDefaultAsync(q => q.ConsultationId == consultationId, ct);

    public Task AddAsync(QueueEntry entry, CancellationToken ct = default)
    {
        db.QueueEntries.Add(entry);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(QueueEntry entry, CancellationToken ct = default)
    {
        db.MarkNewChildrenAdded(entry);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<QueueEntrySummaryDto>> SearchAsync(
        string? clinicType, string? status, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = db.QueueEntries.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(clinicType))
            query = query.Where(q => q.ClinicType == clinicType);
        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<QueueStatus>(status, true, out var parsed))
            query = query.Where(q => q.Status == parsed);

        return await query
            .OrderBy(q => q.Priority) // Routine(0) < Urgent(1) < Emergency(2) — urgent first
            .ThenBy(q => q.RequestedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(q => new QueueEntrySummaryDto(
                q.Id, q.PatientId, q.ClinicType, q.Priority.ToString(), q.Status.ToString(),
                q.QueueNumber, q.Notes, q.RequestedByUserId, q.RequestedAtUtc,
                q.AcceptedByUserId, q.AcceptedAtUtc, q.ConsultationId))
            .ToListAsync(ct);
    }

    public Task<int> CountAsync(string? clinicType, string? status, CancellationToken ct = default)
    {
        var query = db.QueueEntries.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(clinicType))
            query = query.Where(q => q.ClinicType == clinicType);
        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<QueueStatus>(status, true, out var parsed))
            query = query.Where(q => q.Status == parsed);
        return query.CountAsync(ct);
    }

    public async Task<int> NextSequenceAsync(Guid facilityId, string clinicType, DateOnly date, CancellationToken ct = default)
    {
        var start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = start.AddDays(1);

        var last = await db.QueueEntries.AsNoTracking()
            .Where(q => q.FacilityId.Value == facilityId
                        && q.ClinicType == clinicType
                        && q.RequestedAtUtc >= start
                        && q.RequestedAtUtc < end)
            .OrderByDescending(q => q.QueueNumber)
            .Select(q => q.QueueNumber)
            .FirstOrDefaultAsync(ct);

        if (string.IsNullOrEmpty(last)) return 1;

        var dash = last.LastIndexOf('-');
        return dash >= 0 && int.TryParse(last[(dash + 1)..], out var seq) ? seq + 1 : 1;
    }
}
