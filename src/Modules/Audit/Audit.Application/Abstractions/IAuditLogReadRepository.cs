using Jacana.Audit.Application.DTOs;

namespace Jacana.Audit.Application.Abstractions;

/// <summary>Read-only audit trail query. AuditLogEntry is append-only — no write path exists.</summary>
public interface IAuditLogReadRepository
{
    Task<IReadOnlyList<AuditLogEntryDto>> SearchAsync(
        string? entityType, Guid? entityId, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<int> CountAsync(string? entityType, CancellationToken ct = default);
}
