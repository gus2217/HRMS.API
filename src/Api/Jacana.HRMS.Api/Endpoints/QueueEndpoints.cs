using Jacana.Clinical.Application.Features.Queue;
using Jacana.Clinical.Application.DTOs;
using Jacana.Identity.Application;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.HRMS.Api.Endpoints;

/// <summary>
/// Consultation queue endpoints: reception queues patients by clinic,
/// clinicians accept → the consultation is registered atomically.
/// </summary>
public static class QueueEndpoints
{
    public static IEndpointRouteBuilder MapQueueEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/queue");

        group.MapPost("/", CreateAsync)
            .RequireAuthorization(Permissions.Queue.Create);

        group.MapGet("/", SearchAsync)
            .RequireAuthorization(Permissions.Queue.View);

        group.MapPost("/{id:guid}/accept", AcceptAsync)
            .RequireAuthorization(Permissions.Queue.Accept);

        group.MapPost("/{id:guid}/cancel", CancelAsync)
            .RequireAuthorization(Permissions.Queue.Create);

        return app;
    }

    private static async Task<IResult> CreateAsync(
        CreateQueueEntryRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new CreateQueueEntryCommand(
            request.PatientId, request.ClinicType, request.Priority, request.Notes), ct);
        return result.IsSuccess
            ? Results.Created($"/api/v1/queue/{result.Value.Id}", result.Value)
            : MapError(result.Error);
    }

    private static async Task<IResult> SearchAsync(
        ISender sender, CancellationToken ct, string? clinicType = null, string? status = null,
        int pageNumber = 1, int pageSize = 50)
    {
        var result = await sender.Send(new SearchQueueEntriesQuery(
            clinicType, status, pageNumber, pageSize), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> AcceptAsync(
        Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new AcceptQueueEntryCommand(id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> CancelAsync(
        Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new CancelQueueEntryCommand(id), ct);
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
