using Jacana.Identity.Application;
using Jacana.Inpatient.Application.Features.Inpatient;
using Jacana.Inpatient.Application.DTOs;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.HRMS.Api.Endpoints;

/// <summary>Inpatient endpoints: bind → dispatch → map result → return.</summary>
public static class InpatientEndpoints
{
    public static IEndpointRouteBuilder MapInpatientEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/inpatient");

        group.MapPost("/admissions", AdmitAsync)
            .RequireAuthorization(Permissions.Clinical.Consult);

        group.MapGet("/admissions", SearchAdmissionsAsync)
            .RequireAuthorization(Permissions.Clinical.View);

        group.MapGet("/admissions/{id:guid}", GetAdmissionAsync)
            .RequireAuthorization(Permissions.Clinical.View);

        group.MapPost("/admissions/{id:guid}/discharge", DischargeAsync)
            .RequireAuthorization(Permissions.Clinical.Consult);

        group.MapPost("/admissions/{id:guid}/notes", AddWardNoteAsync)
            .RequireAuthorization(Permissions.Clinical.Consult);

        group.MapGet("/ward-occupancy", GetWardOccupancyAsync)
            .RequireAuthorization(Permissions.Clinical.View);

        return app;
    }

    private static async Task<IResult> AdmitAsync(
        AdmitPatientRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new AdmitPatientCommand(
            request.PatientId, request.AdmittingClinicianUserId, request.WardName, request.BedNumber), ct);
        return result.IsSuccess ? Results.Created($"/api/v1/inpatient/admissions/{result.Value.Id}", result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> SearchAdmissionsAsync(
        ISender sender, CancellationToken ct, bool activeOnly = true, Guid? patientId = null, int pageNumber = 1, int pageSize = 20)
    {
        var result = await sender.Send(new SearchAdmissionsQuery(pageNumber, pageSize, activeOnly, patientId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> GetAdmissionAsync(Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetAdmissionQuery(id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> DischargeAsync(Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new DischargePatientCommand(id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> AddWardNoteAsync(
        Guid id, AddWardNoteRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new AddWardNoteCommand(id, request.Content), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> GetWardOccupancyAsync(ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetWardOccupancyQuery(), ct);
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
