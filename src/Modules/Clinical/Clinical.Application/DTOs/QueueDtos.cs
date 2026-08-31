namespace Jacana.Clinical.Application.DTOs;

/// <summary>Queue entry row with patient identity resolved cross-schema.</summary>
public sealed record QueueEntryDto(
    Guid Id,
    Guid PatientId,
    string PatientNumber,
    string PatientName,
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

/// <summary>Result of accepting a queue entry — includes the registered consultation.</summary>
public sealed record AcceptQueueEntryResponseDto(
    QueueEntryDto QueueEntry,
    Guid ConsultationId);
