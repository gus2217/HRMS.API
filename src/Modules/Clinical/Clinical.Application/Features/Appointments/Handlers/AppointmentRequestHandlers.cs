using Jacana.Clinical.Application.Abstractions;
using Jacana.Clinical.Application.DTOs;
using Jacana.Clinical.Domain;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Clinical.Application.Features.Appointments.Handlers;

/// <summary>Reception raises a request for an appointment at a clinic.</summary>
public sealed class CreateAppointmentRequestCommandHandler(
    IAppointmentRequestRepository requests,
    ICurrentUser currentUser,
    IClock clock,
    IPatientIdentityLookup patients,
    IUserIdentityLookup users)
    : IRequestHandler<CreateAppointmentRequestCommand, Result<AppointmentRequestDto>>
{
    public async Task<Result<AppointmentRequestDto>> Handle(CreateAppointmentRequestCommand request, CancellationToken ct)
    {
        var entry = AppointmentRequest.Create(
            Guid.NewGuid(), currentUser.FacilityId, request.PatientId, request.ClinicType,
            request.Reason, request.Notes, request.PreferredDate, currentUser.UserId, clock.UtcNow);
        if (entry.IsFailure) return entry.Error;

        await requests.AddAsync(entry.Value, ct);

        var patient = await ResolvePatient(entry.Value.PatientId, patients, ct);
        var requester = await ResolveUser(entry.Value.RequestedByUserId, users, ct);
        return ToDto(entry.Value, patient, requester, null);
    }

    private static async Task<PatientIdentityDto?> ResolvePatient(
        Guid patientId, IPatientIdentityLookup patients, CancellationToken ct)
    {
        var map = await patients.GetIdentitiesAsync([patientId], ct);
        return map.TryGetValue(patientId, out var p) ? p : null;
    }

    private static async Task<UserIdentityDto?> ResolveUser(
        Guid userId, IUserIdentityLookup users, CancellationToken ct)
    {
        var map = await users.GetIdentitiesAsync([userId], ct);
        return map.TryGetValue(userId, out var u) ? u : null;
    }

    private static AppointmentRequestDto ToDto(
        AppointmentRequest r, PatientIdentityDto? patient, UserIdentityDto? requester, UserIdentityDto? approver)
        => new(
            r.Id, r.PatientId, patient?.PatientNumber ?? string.Empty, patient?.FullName ?? string.Empty,
            r.ClinicType, r.Reason, r.Notes, r.PreferredDate, r.Status.ToString(),
            r.RequestedByUserId, requester?.FullName ?? string.Empty, r.RequestedAtUtc,
            r.ApprovedByUserId, approver?.FullName, r.ApprovedAtUtc, r.AppointmentId);
}

/// <summary>Clinician approves a request and schedules the real appointment.</summary>
public sealed class ApproveAppointmentRequestCommandHandler(
    IAppointmentRequestRepository requests,
    IAppointmentRepository appointments,
    ICurrentUser currentUser,
    IClock clock,
    IPatientIdentityLookup patients)
    : IRequestHandler<ApproveAppointmentRequestCommand, Result<AppointmentDto>>
{
    public async Task<Result<AppointmentDto>> Handle(ApproveAppointmentRequestCommand request, CancellationToken ct)
    {
        var entry = await requests.GetByIdAsync(request.RequestId, ct);
        if (entry is null) return Error.NotFound("Appointment request not found.");
        if (entry.Status != AppointmentRequestStatus.Pending)
            return Error.InvalidOperation($"Request is {entry.Status}, not pending.");

        if (!Enum.TryParse<AppointmentType>(request.Type, true, out var type))
            return Error.Validation($"Unknown appointment type '{request.Type}'.");

        var appt = Appointment.Create(
            Guid.NewGuid(), currentUser.FacilityId, entry.PatientId, entry.ClinicType,
            type, request.ScheduledAtUtc, request.DurationMinutes, entry.Reason,
            null, RecurrencePattern.None, currentUser.UserId, clock.UtcNow);
        if (appt.IsFailure) return appt.Error;

        var approve = entry.Approve(currentUser.UserId, appt.Value.Id, clock.UtcNow);
        if (approve.IsFailure) return approve.Error;

        await appointments.AddAsync(appt.Value, ct);
        await requests.UpdateAsync(entry, ct);

        var identities = await patients.GetIdentitiesAsync([appt.Value.PatientId], ct);
        identities.TryGetValue(appt.Value.PatientId, out var patient);
        var a = appt.Value;
        return new AppointmentDto(
            a.Id, a.PatientId, patient?.PatientNumber ?? string.Empty, patient?.FullName ?? string.Empty,
            a.ClinicType, a.Type.ToString(), a.Status.ToString(), a.ScheduledAtUtc, a.DurationMinutes,
            a.Reason, a.RecurrenceGroupId, a.RecurrencePattern.ToString(),
            a.CreatedByUserId, a.CreatedAtUtc, a.ConsultationId, a.StartedAtUtc, a.CompletedAtUtc);
    }
}

/// <summary>Clinician declines a request.</summary>
public sealed class DeclineAppointmentRequestCommandHandler(
    IAppointmentRequestRepository requests,
    ICurrentUser currentUser,
    IClock clock,
    IPatientIdentityLookup patients,
    IUserIdentityLookup users)
    : IRequestHandler<DeclineAppointmentRequestCommand, Result<AppointmentRequestDto>>
{
    public async Task<Result<AppointmentRequestDto>> Handle(DeclineAppointmentRequestCommand request, CancellationToken ct)
    {
        var entry = await requests.GetByIdAsync(request.RequestId, ct);
        if (entry is null) return Error.NotFound("Appointment request not found.");

        var decline = entry.Decline(currentUser.UserId, clock.UtcNow);
        if (decline.IsFailure) return decline.Error;

        await requests.UpdateAsync(entry, ct);

        var patient = await ResolvePatient(entry.PatientId, patients, ct);
        var requester = await ResolveUser(entry.RequestedByUserId, users, ct);
        var approver = await ResolveUser(currentUser.UserId, users, ct);
        return new AppointmentRequestDto(
            entry.Id, entry.PatientId, patient?.PatientNumber ?? string.Empty, patient?.FullName ?? string.Empty,
            entry.ClinicType, entry.Reason, entry.Notes, entry.PreferredDate, entry.Status.ToString(),
            entry.RequestedByUserId, requester?.FullName ?? string.Empty, entry.RequestedAtUtc,
            entry.ApprovedByUserId, approver?.FullName, entry.ApprovedAtUtc, entry.AppointmentId);
    }

    private static async Task<PatientIdentityDto?> ResolvePatient(
        Guid patientId, IPatientIdentityLookup patients, CancellationToken ct)
    {
        var map = await patients.GetIdentitiesAsync([patientId], ct);
        return map.TryGetValue(patientId, out var p) ? p : null;
    }

    private static async Task<UserIdentityDto?> ResolveUser(
        Guid userId, IUserIdentityLookup users, CancellationToken ct)
    {
        var map = await users.GetIdentitiesAsync([userId], ct);
        return map.TryGetValue(userId, out var u) ? u : null;
    }
}

/// <summary>Requests board — reception + clinicians, filterable by clinic/status.</summary>
public sealed class SearchAppointmentRequestsQueryHandler(
    IAppointmentRequestRepository requests,
    IPatientIdentityLookup patients,
    IUserIdentityLookup users)
    : IRequestHandler<SearchAppointmentRequestsQuery, Result<PagedResult<AppointmentRequestDto>>>
{
    public async Task<Result<PagedResult<AppointmentRequestDto>>> Handle(SearchAppointmentRequestsQuery request, CancellationToken ct)
    {
        var items = await requests.SearchAsync(request.ClinicType, request.Status, request.PageNumber, request.PageSize, ct);
        var total = await requests.CountAsync(request.ClinicType, request.Status, ct);

        var patientIds = items.Select(i => i.PatientId).Distinct().ToArray();
        var userIds = items
            .SelectMany(i => new[] { i.RequestedByUserId, i.ApprovedByUserId }.OfType<Guid>())
            .Distinct().ToArray();

        var pLookup = await patients.GetIdentitiesAsync(patientIds, ct);
        var uLookup = await users.GetIdentitiesAsync(userIds, ct);

        var rows = items.Select(i =>
        {
            pLookup.TryGetValue(i.PatientId, out var patient);
            uLookup.TryGetValue(i.RequestedByUserId, out var requestedBy);
            Guid? approvedById = i.ApprovedByUserId;
            UserIdentityDto? approvedBy = approvedById is not null && uLookup.TryGetValue(approvedById.Value, out var u) ? u : null;

            return new AppointmentRequestDto(
                i.Id, i.PatientId, patient?.PatientNumber ?? string.Empty, patient?.FullName ?? string.Empty,
                i.ClinicType, i.Reason, i.Notes, i.PreferredDate, i.Status,
                i.RequestedByUserId, requestedBy?.FullName ?? string.Empty, i.RequestedAtUtc,
                i.ApprovedByUserId, approvedBy?.FullName, i.ApprovedAtUtc, i.AppointmentId);
        }).ToArray();

        return Result.Success(new PagedResult<AppointmentRequestDto>(rows, total, request.PageNumber, request.PageSize));
    }
}
