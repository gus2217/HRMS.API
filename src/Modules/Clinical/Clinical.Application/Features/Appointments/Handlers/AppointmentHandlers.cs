using Jacana.Clinical.Application.Abstractions;
using Jacana.Clinical.Application.DTOs;
using Jacana.Clinical.Domain;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Clinical.Application.Features.Appointments.Handlers;

/// <summary>Reception/clinician books an appointment (optionally a recurring series).</summary>
public sealed class CreateAppointmentCommandHandler(
    IAppointmentRepository appointments,
    ICurrentUser currentUser,
    IClock clock)
    : IRequestHandler<CreateAppointmentCommand, Result<IReadOnlyList<AppointmentDto>>>
{
    public async Task<Result<IReadOnlyList<AppointmentDto>>> Handle(CreateAppointmentCommand request, CancellationToken ct)
    {
        if (!Enum.TryParse<AppointmentType>(request.Type, true, out var type))
            return Error.Validation($"Unknown appointment type '{request.Type}'.");

        var pattern = string.IsNullOrWhiteSpace(request.RecurrencePattern)
            ? RecurrencePattern.None
            : Enum.TryParse<RecurrencePattern>(request.RecurrencePattern, true, out var p)
                ? p
                : RecurrencePattern.None;

        var count = pattern == RecurrencePattern.None ? 1 : Math.Clamp(request.RecurrenceCount, 1, 52);
        Guid? groupId = pattern == RecurrencePattern.None ? null : Guid.NewGuid();
        var created = new List<Appointment>();

        for (var i = 0; i < count; i++)
        {
            var scheduledAt = AddInterval(request.ScheduledAtUtc, pattern, i);

            // Stop early once the recurrence end date is passed.
            if (request.RecurrenceEndDate is not null
                && DateOnly.FromDateTime(scheduledAt) > request.RecurrenceEndDate)
                break;

            // Double-booking guard: no overlapping active appointment in this clinic.
            var fromUtc = scheduledAt;
            var toUtc = scheduledAt.AddMinutes(request.DurationMinutes);
            if (await appointments.HasOverlapAsync(request.ClinicType, fromUtc, toUtc, ct))
                return Error.InvalidOperation(
                    $"{request.ClinicType} is already booked for {scheduledAt:HH:mm} ({(i == 0 ? "" : $"occurrence {i + 1}, ")}conflict with an existing appointment).");

            var appt = Appointment.Create(
                Guid.NewGuid(), currentUser.FacilityId, request.PatientId, request.ClinicType,
                type, scheduledAt, request.DurationMinutes, request.Reason,
                groupId, pattern, currentUser.UserId, clock.UtcNow);
            if (appt.IsFailure) return appt.Error;

            await appointments.AddAsync(appt.Value, ct);
            created.Add(appt.Value);
        }

        return created.Select(Map).ToArray();
    }

    private static DateTime AddInterval(DateTime start, RecurrencePattern pattern, int index)
    {
        if (index == 0) return start;
        return pattern switch
        {
            RecurrencePattern.Daily => start.AddDays(index),
            RecurrencePattern.Weekly => start.AddDays(index * 7),
            RecurrencePattern.Monthly => start.AddMonths(index),
            _ => start
        };
    }

    internal static AppointmentDto Map(Appointment a) => new(
        a.Id, a.PatientId, string.Empty, string.Empty, a.ClinicType,
        a.Type.ToString(), a.Status.ToString(), a.ScheduledAtUtc, a.DurationMinutes,
        a.Reason, a.RecurrenceGroupId, a.RecurrencePattern.ToString(),
        a.CreatedByUserId, a.CreatedAtUtc, a.ConsultationId, a.StartedAtUtc, a.CompletedAtUtc);
}

/// <summary>Clinician starts an appointment → registers the consultation (Source=Appointment).</summary>
public sealed class StartAppointmentCommandHandler(
    IAppointmentRepository appointments,
    IConsultationRepository consultations,
    ICurrentUser currentUser,
    IClock clock,
    IPatientIdentityLookup patients)
    : IRequestHandler<StartAppointmentCommand, Result<StartAppointmentResponseDto>>
{
    public async Task<Result<StartAppointmentResponseDto>> Handle(StartAppointmentCommand request, CancellationToken ct)
    {
        var appt = await appointments.GetByIdAsync(request.AppointmentId, ct);
        if (appt is null) return Error.NotFound("Appointment not found.");
        if (appt.Status != AppointmentStatus.Scheduled)
            return Error.InvalidOperation($"Appointment is {appt.Status}, not scheduled.");

        var consultation = Consultation.Start(
            Guid.NewGuid(), currentUser.FacilityId, appt.PatientId, currentUser.UserId, clock.UtcNow);
        if (consultation.IsFailure) return consultation.Error;
        consultation.Value.SetSource(ConsultationSource.Appointment, appt.Id);

        var start = appt.Start(consultation.Value.Id, clock.UtcNow);
        if (start.IsFailure) return start.Error;

        await consultations.AddAsync(consultation.Value, ct);
        await appointments.UpdateAsync(appt, ct);

        var dto = await ToDtoAsync(appt, ct);
        return new StartAppointmentResponseDto(dto, consultation.Value.Id);
    }

    private async Task<AppointmentDto> ToDtoAsync(Appointment a, CancellationToken ct)
    {
        var identities = await patients.GetIdentitiesAsync([a.PatientId], ct);
        identities.TryGetValue(a.PatientId, out var patient);
        return new AppointmentDto(
            a.Id, a.PatientId, patient?.PatientNumber ?? string.Empty, patient?.FullName ?? string.Empty,
            a.ClinicType, a.Type.ToString(), a.Status.ToString(), a.ScheduledAtUtc, a.DurationMinutes,
            a.Reason, a.RecurrenceGroupId, a.RecurrencePattern.ToString(),
            a.CreatedByUserId, a.CreatedAtUtc, a.ConsultationId, a.StartedAtUtc, a.CompletedAtUtc);
    }
}

/// <summary>Manually completes an appointment (no linked consultation needed).</summary>
public sealed class CompleteAppointmentCommandHandler(
    IAppointmentRepository appointments,
    IPatientIdentityLookup patients)
    : IRequestHandler<CompleteAppointmentCommand, Result<AppointmentDto>>
{
    public async Task<Result<AppointmentDto>> Handle(CompleteAppointmentCommand request, CancellationToken ct)
    {
        var appt = await appointments.GetByIdAsync(request.AppointmentId, ct);
        if (appt is null) return Error.NotFound("Appointment not found.");

        var result = appt.Complete(DateTime.UtcNow);
        if (result.IsFailure) return result.Error;

        await appointments.UpdateAsync(appt, ct);

        var identities = await patients.GetIdentitiesAsync([appt.PatientId], ct);
        identities.TryGetValue(appt.PatientId, out var patient);
        return new AppointmentDto(
            appt.Id, appt.PatientId, patient?.PatientNumber ?? string.Empty, patient?.FullName ?? string.Empty,
            appt.ClinicType, appt.Type.ToString(), appt.Status.ToString(), appt.ScheduledAtUtc, appt.DurationMinutes,
            appt.Reason, appt.RecurrenceGroupId, appt.RecurrencePattern.ToString(),
            appt.CreatedByUserId, appt.CreatedAtUtc, appt.ConsultationId, appt.StartedAtUtc, appt.CompletedAtUtc);
    }
}

/// <summary>Cancels an appointment.</summary>
public sealed class CancelAppointmentCommandHandler(
    IAppointmentRepository appointments,
    IPatientIdentityLookup patients)
    : IRequestHandler<CancelAppointmentCommand, Result<AppointmentDto>>
{
    public async Task<Result<AppointmentDto>> Handle(CancelAppointmentCommand request, CancellationToken ct)
    {
        var appt = await appointments.GetByIdAsync(request.AppointmentId, ct);
        if (appt is null) return Error.NotFound("Appointment not found.");

        var result = appt.Cancel(DateTime.UtcNow);
        if (result.IsFailure) return result.Error;

        await appointments.UpdateAsync(appt, ct);

        var identities = await patients.GetIdentitiesAsync([appt.PatientId], ct);
        identities.TryGetValue(appt.PatientId, out var patient);
        return new AppointmentDto(
            appt.Id, appt.PatientId, patient?.PatientNumber ?? string.Empty, patient?.FullName ?? string.Empty,
            appt.ClinicType, appt.Type.ToString(), appt.Status.ToString(), appt.ScheduledAtUtc, appt.DurationMinutes,
            appt.Reason, appt.RecurrenceGroupId, appt.RecurrencePattern.ToString(),
            appt.CreatedByUserId, appt.CreatedAtUtc, appt.ConsultationId, appt.StartedAtUtc, appt.CompletedAtUtc);
    }
}

/// <summary>Marks a scheduled appointment as a no-show.</summary>
public sealed class NoShowAppointmentCommandHandler(
    IAppointmentRepository appointments,
    IPatientIdentityLookup patients)
    : IRequestHandler<NoShowAppointmentCommand, Result<AppointmentDto>>
{
    public async Task<Result<AppointmentDto>> Handle(NoShowAppointmentCommand request, CancellationToken ct)
    {
        var appt = await appointments.GetByIdAsync(request.AppointmentId, ct);
        if (appt is null) return Error.NotFound("Appointment not found.");

        var result = appt.MarkNoShow(DateTime.UtcNow);
        if (result.IsFailure) return result.Error;

        await appointments.UpdateAsync(appt, ct);

        var identities = await patients.GetIdentitiesAsync([appt.PatientId], ct);
        identities.TryGetValue(appt.PatientId, out var patient);
        return new AppointmentDto(
            appt.Id, appt.PatientId, patient?.PatientNumber ?? string.Empty, patient?.FullName ?? string.Empty,
            appt.ClinicType, appt.Type.ToString(), appt.Status.ToString(), appt.ScheduledAtUtc, appt.DurationMinutes,
            appt.Reason, appt.RecurrenceGroupId, appt.RecurrencePattern.ToString(),
            appt.CreatedByUserId, appt.CreatedAtUtc, appt.ConsultationId, appt.StartedAtUtc, appt.CompletedAtUtc);
    }
}

/// <summary>Appointments list (day queue / filtered board).</summary>
public sealed class SearchAppointmentsQueryHandler(
    IAppointmentRepository appointments,
    IPatientIdentityLookup patients)
    : IRequestHandler<SearchAppointmentsQuery, Result<PagedResult<AppointmentDto>>>
{
    public async Task<Result<PagedResult<AppointmentDto>>> Handle(SearchAppointmentsQuery request, CancellationToken ct)
    {
        var items = await appointments.SearchAsync(
            request.ClinicType, request.Status, request.FromUtc, request.ToUtc,
            request.PageNumber, request.PageSize, ct);
        var total = await appointments.CountAsync(
            request.ClinicType, request.Status, request.FromUtc, request.ToUtc, ct);

        var identities = await patients.GetIdentitiesAsync(items.Select(i => i.PatientId).ToArray(), ct);
        var rows = items.Select(i => ToDto(i, identities)).ToArray();

        return Result.Success(new PagedResult<AppointmentDto>(rows, total, request.PageNumber, request.PageSize));
    }

    private static AppointmentDto ToDto(
        AppointmentSummaryDto i, IReadOnlyDictionary<Guid, PatientIdentityDto> identities)
    {
        identities.TryGetValue(i.PatientId, out var patient);
        return new AppointmentDto(
            i.Id, i.PatientId, patient?.PatientNumber ?? string.Empty, patient?.FullName ?? string.Empty,
            i.ClinicType, i.Type, i.Status, i.ScheduledAtUtc, i.DurationMinutes,
            i.Reason, i.RecurrenceGroupId, i.RecurrencePattern,
            i.CreatedByUserId, i.CreatedAtUtc, i.ConsultationId, i.StartedAtUtc, i.CompletedAtUtc);
    }
}

/// <summary>Month calendar view — appointments for a month, clinic-filterable.</summary>
public sealed class GetAppointmentsByMonthQueryHandler(
    IAppointmentRepository appointments,
    IPatientIdentityLookup patients)
    : IRequestHandler<GetAppointmentsByMonthQuery, Result<IReadOnlyList<AppointmentDto>>>
{
    public async Task<Result<IReadOnlyList<AppointmentDto>>> Handle(GetAppointmentsByMonthQuery request, CancellationToken ct)
    {
        var items = await appointments.GetByMonthAsync(request.Year, request.Month, request.ClinicType, ct);
        var identities = await patients.GetIdentitiesAsync(items.Select(i => i.PatientId).ToArray(), ct);

        return items.Select(i =>
        {
            identities.TryGetValue(i.PatientId, out var patient);
            return new AppointmentDto(
                i.Id, i.PatientId, patient?.PatientNumber ?? string.Empty, patient?.FullName ?? string.Empty,
                i.ClinicType, i.Type, i.Status, i.ScheduledAtUtc, i.DurationMinutes,
                i.Reason, i.RecurrenceGroupId, i.RecurrencePattern,
                i.CreatedByUserId, i.CreatedAtUtc, i.ConsultationId, i.StartedAtUtc, i.CompletedAtUtc);
        }).ToArray();
    }
}
