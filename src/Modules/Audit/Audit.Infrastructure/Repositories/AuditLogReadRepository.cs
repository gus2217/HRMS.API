using Jacana.Audit.Application.Abstractions;
using Jacana.Audit.Application.DTOs;
using Jacana.Audit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jacana.Audit.Infrastructure.Repositories;

public sealed class AuditLogReadRepository(AuditDbContext db) : IAuditLogReadRepository
{
    public async Task<IReadOnlyList<AuditLogEntryDto>> SearchAsync(
        string? entityType, Guid? entityId, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = db.AuditLog.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(a => a.EntityType == entityType);

        return await query
            .OrderByDescending(a => a.PerformedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogEntryDto(
                a.Id, a.FacilityId, a.EntityType, a.EntityId, a.Action.ToString(),
                a.PerformedByUserId, a.PerformedAtUtc, a.BeforeValuesJson, a.AfterValuesJson))
            .ToListAsync(ct);
    }

    public Task<int> CountAsync(string? entityType, CancellationToken ct = default)
    {
        var query = db.AuditLog.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(a => a.EntityType == entityType);
        return query.CountAsync(ct);
    }
}
