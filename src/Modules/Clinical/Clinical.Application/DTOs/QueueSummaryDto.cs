namespace Jacana.Clinical.Application.DTOs;

/// <summary>Repository-level summary row (no patient identity — resolved via lookup).</summary>
public sealed record QueueEntrySummaryDto(
    Guid Id,
    Guid PatientId,
    string ClinicType,
    string Priority,
    string Status,
    string QueueNumber,
    string? Notes,
    Guid RequestedByUserId,
    DateTime RequestedAtUtc,
    Guid? AcceptedByUserId,
    DateTime? AcceptedAtUtc,
    Guid? ConsultationId);
