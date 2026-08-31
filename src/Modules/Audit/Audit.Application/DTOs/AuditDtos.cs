namespace Jacana.Audit.Application.DTOs;

public sealed record AuditLogEntryDto(
    Guid Id,
    Guid FacilityId,
    string EntityType,
    string EntityId,
    string? EntityName,
    string Action,
    Guid PerformedByUserId,
    string? PerformedByName,
    DateTime PerformedAtUtc,
    string? BeforeValuesJson,
    string? AfterValuesJson);
