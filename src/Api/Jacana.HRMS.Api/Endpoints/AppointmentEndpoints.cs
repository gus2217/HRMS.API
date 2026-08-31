using Jacana.Clinical.Application.Features.Appointments;
using Jacana.Clinical.Application.DTOs;
using Jacana.Identity.Application;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.HRMS.Api.Endpoints;

/// <summary>
/// Appointment endpoints: clinicians book/start/complete appointments (optionally
/// recurring), reception raises approval requests, clinicians approve them.
/// </summary>
public static class AppointmentEndpoints
{
    public static IEndpointRouteBuilder MapAppointmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/appointments");

        group.MapPost("/", CreateAsync)
            .RequireAuthorization(Permissions.Appointment.Create);

        group.MapGet("/", SearchAsync)
            .RequireAuthorization(Permissions.Appointment.View);

        group.MapGet("/calendar", GetByMonthAsync)
            .RequireAuthorization(Permissions.Appointment.View);

        group.MapPost("/{id:guid}/start", StartAsync)
            .RequireAuthorization(Permissions.Appointment.Create);

        group.MapPost("/{id:guid}/complete", CompleteAsync)
            .RequireAuthorization(Permissions.Appointment.Create);

        group.MapPost("/{id:guid}/cancel", CancelAsync)
            .RequireAuthorization(Permissions.Appointment.Create);

        group.MapPost("/{id:guid}/no-show", NoShowAsync)
            .RequireAuthorization(Permissions.Appointment.Create);

        var requests = app.MapGroup("/api/v1/appointment-requests");

        requests.MapPost("/", CreateRequestAsync)
            .RequireAuthorization(Permissions.Appointment.Request);

        requests.MapGet("/", SearchRequestsAsync)
            .RequireAuthorization(Permissions.Appointment.View);

        requests.MapPost("/{id:guid}/approve", ApproveAsync)
            .RequireAuthorization(Permissions.Appointment.Approve);

        requests.MapPost("/{id:guid}/decline", DeclineAsync)
            .RequireAuthorization(Permissions.Appointment.Approve);

        return app;
    }

    private static async Task<IResult> CreateAsync(
        CreateAppointmentRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new CreateAppointmentCommand(
            request.PatientId, request.ClinicType, request.Type, request.ScheduledAtUtc,
            request.DurationMinutes, request.Reason, request.PreviousConsultationId,
            request.RecurrencePattern, request.RecurrenceCount, request.RecurrenceEndDate), ct);
        return result.IsSuccess
            ? Results.Created($"/api/v1/appointments", result.Value)
            : MapError(result.Error);
    }

    private static async Task<IResult> SearchAsync(
        ISender sender, CancellationToken ct, string? clinicType = null, string? status = null,
        DateTime? fromUtc = null, DateTime? toUtc = null, int pageNumber = 1, int pageSize = 50)
    {
        var result = await sender.Send(new SearchAppointmentsQuery(
            clinicType, status, fromUtc, toUtc, pageNumber, pageSize), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> GetByMonthAsync(
        ISender sender, CancellationToken ct, int year, int month, string? clinicType = null)
    {
        var result = await sender.Send(new GetAppointmentsByMonthQuery(year, month, clinicType), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> StartAsync(Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new StartAppointmentCommand(id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> CompleteAsync(Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new CompleteAppointmentCommand(id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> CancelAsync(Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new CancelAppointmentCommand(id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> NoShowAsync(Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new NoShowAppointmentCommand(id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> CreateRequestAsync(
        CreateAppointmentRequestRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new CreateAppointmentRequestCommand(
            request.PatientId, request.ClinicType, request.Reason, request.Notes, request.PreferredDate), ct);
        return result.IsSuccess
            ? Results.Created($"/api/v1/appointment-requests/{result.Value.Id}", result.Value)
            : MapError(result.Error);
    }

    private static async Task<IResult> SearchRequestsAsync(
        ISender sender, CancellationToken ct, string? clinicType = null, string? status = null,
        int pageNumber = 1, int pageSize = 50)
    {
        var result = await sender.Send(new SearchAppointmentRequestsQuery(
            clinicType, status, pageNumber, pageSize), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> ApproveAsync(
        Guid id, ApproveAppointmentRequestRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ApproveAppointmentRequestCommand(
            id, request.ScheduledAtUtc, request.DurationMinutes, request.Type, request.PreviousConsultationId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> DeclineAsync(Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new DeclineAppointmentRequestCommand(id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static IResult MapError(Error error) => error.Code switch
    {
        ErrorCodes.NotFound => Results.NotFound(new { error = error.Message }),
        ErrorCodes.InvalidOperation => Results.BadRequest(new { error = error.Message }),
        ErrorCodes.Validation => Results.BadRequest(new { error = error.Message }),
        ErrorCodes.Forbidden => Results.Forbid(),
        ErrorCodes.Unauthorized => Results.Unauthorized(),
        _ => Results.BadRequest(new { error = error.Message })
    };
}
