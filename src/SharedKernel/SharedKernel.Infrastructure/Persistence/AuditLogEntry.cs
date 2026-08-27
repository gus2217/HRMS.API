namespace Jacana.SharedKernel.Infrastructure.Persistence;

/// <summary>
/// Append-only, immutable audit record. No update/delete path exists in code —
/// there is deliberately no way to mutate or remove an entry after it is written.
/// </summary>
public sealed class AuditLogEntry
{
    public Guid Id { get; set; }
    public Guid FacilityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public AuditAction Action { get; set; }
    public string? BeforeValuesJson { get; set; }
    public string? AfterValuesJson { get; set; }
    public Guid PerformedByUserId { get; set; }
    public DateTime PerformedAtUtc { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}
