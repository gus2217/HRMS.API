namespace Jacana.Audit.Application.DTOs;

public sealed record AuditLogEntryDto(
    Guid Id,
    Guid FacilityId,
    string EntityType,
    string EntityId,
    string Action,
    Guid PerformedByUserId,
    DateTime PerformedAtUtc,
    string? BeforeValuesJson,
    string? AfterValuesJson);
