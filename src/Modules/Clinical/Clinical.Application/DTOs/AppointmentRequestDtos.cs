namespace Jacana.Clinical.Application.DTOs;

/// <summary>HTTP request binding for booking an appointment (optionally recurring).</summary>
public sealed record CreateAppointmentRequestDto(
    Guid PatientId,
    string ClinicType,
    string Type,
    DateTime ScheduledAtUtc,
    int DurationMinutes,
    string? Reason,
    Guid? PreviousConsultationId,
    string? RecurrencePattern,
    int RecurrenceCount,
    DateOnly? RecurrenceEndDate);

/// <summary>HTTP request binding for a reception appointment request.</summary>
public sealed record CreateAppointmentRequestRequestDto(
    Guid PatientId,
    string ClinicType,
    string Reason,
    string? Notes,
    DateOnly? PreferredDate);

/// <summary>HTTP request binding for approving a request.</summary>
public sealed record ApproveAppointmentRequestRequestDto(
    DateTime ScheduledAtUtc,
    int DurationMinutes,
    string Type,
    Guid? PreviousConsultationId);
