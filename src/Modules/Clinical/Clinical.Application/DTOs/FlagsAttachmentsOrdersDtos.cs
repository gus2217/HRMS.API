namespace Jacana.Clinical.Application.DTOs;

/// <summary>A patient flag (allergy alert / warning / info note).</summary>
public sealed record PatientFlagDto(
    Guid Id,
    Guid PatientId,
    string Type,
    string Message,
    bool IsActive,
    Guid CreatedByUserId,
    DateTime CreatedAtUtc,
    Guid? DeactivatedByUserId,
    DateTime? DeactivatedAtUtc);

/// <summary>A document attached to a patient's record (metadata only).</summary>
public sealed record PatientAttachmentDto(
    Guid Id,
    Guid PatientId,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Category,
    Guid UploadedByUserId,
    DateTime UploadedAtUtc);

/// <summary>A diagnostic order (imaging / procedure).</summary>
public sealed record DiagnosticOrderDto(
    Guid Id,
    Guid PatientId,
    Guid? ConsultationId,
    string Type,
    string Name,
    string? BodySite,
    string ClinicalIndication,
    string Priority,
    string Status,
    Guid OrderedByUserId,
    DateTime OrderedAtUtc,
    Guid? ScheduledByUserId,
    DateTime? ScheduledAtUtc,
    Guid? PerformedByUserId,
    DateTime? PerformedAtUtc,
    string? Report,
    Guid? ReportedByUserId,
    DateTime? ReportedAtUtc,
    Guid? CancelledByUserId,
    DateTime? CancelledAtUtc,
    string? CancellationReason);
