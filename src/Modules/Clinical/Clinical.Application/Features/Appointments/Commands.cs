using Jacana.Clinical.Application.DTOs;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;

namespace Jacana.Clinical.Application.Features.Appointments;

public sealed record CreateAppointmentCommand(
    Guid PatientId,
    string ClinicType,
    string Type,
    DateTime ScheduledAtUtc,
    int DurationMinutes,
    string? Reason,
    string? RecurrencePattern,
    int RecurrenceCount,
    DateOnly? RecurrenceEndDate)
    : ICommand<Result<IReadOnlyList<AppointmentDto>>>;

public sealed record StartAppointmentCommand(
    Guid AppointmentId)
    : ICommand<Result<StartAppointmentResponseDto>>;

public sealed record CompleteAppointmentCommand(
    Guid AppointmentId)
    : ICommand<Result<AppointmentDto>>;

public sealed record CancelAppointmentCommand(
    Guid AppointmentId)
    : ICommand<Result<AppointmentDto>>;

public sealed record NoShowAppointmentCommand(
    Guid AppointmentId)
    : ICommand<Result<AppointmentDto>>;

public sealed record SearchAppointmentsQuery(
    string? ClinicType,
    string? Status,
    DateTime? FromUtc,
    DateTime? ToUtc,
    int PageNumber,
    int PageSize)
    : IQuery<Result<PagedResult<AppointmentDto>>>;

public sealed record GetAppointmentsByMonthQuery(
    int Year,
    int Month,
    string? ClinicType)
    : IQuery<Result<IReadOnlyList<AppointmentDto>>>;

// ─── Appointment requests ─────────────────────────────────────────────────────

public sealed record CreateAppointmentRequestCommand(
    Guid PatientId,
    string ClinicType,
    string Reason,
    string? Notes,
    DateOnly? PreferredDate)
    : ICommand<Result<AppointmentRequestDto>>;

public sealed record ApproveAppointmentRequestCommand(
    Guid RequestId,
    DateTime ScheduledAtUtc,
    int DurationMinutes,
    string Type)
    : ICommand<Result<AppointmentDto>>;

public sealed record DeclineAppointmentRequestCommand(
    Guid RequestId)
    : ICommand<Result<AppointmentRequestDto>>;

public sealed record SearchAppointmentRequestsQuery(
    string? ClinicType,
    string? Status,
    int PageNumber,
    int PageSize)
    : IQuery<Result<PagedResult<AppointmentRequestDto>>>;
