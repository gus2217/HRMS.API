namespace Jacana.Clinical.Application.DTOs;

/// <summary>Repository-level appointment row (patient identity resolved via lookup).</summary>
public sealed record AppointmentSummaryDto(
    Guid Id,
    Guid PatientId,
    string ClinicType,
    string Type,
    string Status,
    DateTime ScheduledAtUtc,
    int DurationMinutes,
    string? Reason,
    Guid? PreviousConsultationId,
    Guid? RecurrenceGroupId,
    string RecurrencePattern,
    Guid CreatedByUserId,
    DateTime CreatedAtUtc,
    Guid? ConsultationId,
    DateTime? StartedAtUtc,
    DateTime? CompletedAtUtc);

/// <summary>Appointment row with patient display identity.</summary>
public sealed record AppointmentDto(
    Guid Id,
    Guid PatientId,
    string PatientNumber,
    string PatientName,
    string ClinicType,
    string Type,
    string Status,
    DateTime ScheduledAtUtc,
    int DurationMinutes,
    string? Reason,
    Guid? PreviousConsultationId,
    Guid? RecurrenceGroupId,
    string RecurrencePattern,
    Guid CreatedByUserId,
    DateTime CreatedAtUtc,
    Guid? ConsultationId,
    DateTime? StartedAtUtc,
    DateTime? CompletedAtUtc);

/// <summary>Result of starting an appointment — carries the registered consultation.</summary>
public sealed record StartAppointmentResponseDto(
    AppointmentDto Appointment,
    Guid ConsultationId);

/// <summary>Repository-level request row.</summary>
public sealed record AppointmentRequestSummaryDto(
    Guid Id,
    Guid PatientId,
    string ClinicType,
    string Reason,
    string? Notes,
    DateOnly? PreferredDate,
    string Status,
    Guid RequestedByUserId,
    DateTime RequestedAtUtc,
    Guid? ApprovedByUserId,
    DateTime? ApprovedAtUtc,
    Guid? AppointmentId);

/// <summary>Request row with patient identity + requester identity.</summary>
public sealed record AppointmentRequestDto(
    Guid Id,
    Guid PatientId,
    string PatientNumber,
    string PatientName,
    string ClinicType,
    string Reason,
    string? Notes,
    DateOnly? PreferredDate,
    string Status,
    Guid RequestedByUserId,
    string RequestedByName,
    DateTime RequestedAtUtc,
    Guid? ApprovedByUserId,
    string? ApprovedByName,
    DateTime? ApprovedAtUtc,
    Guid? AppointmentId);
