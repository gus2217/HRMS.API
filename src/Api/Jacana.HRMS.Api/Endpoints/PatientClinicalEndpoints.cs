using Jacana.Clinical.Application.DTOs;
using Jacana.Clinical.Application.Features.PatientClinical;
using Jacana.Identity.Application;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.HRMS.Api.Endpoints;

/// <summary>
/// Patient-scoped clinical summary endpoints (vitals, immunizations, conditions).
/// These mirror OpenMRS's patient-chart widgets and are independent of the
/// consultation aggregate, so a patient's vitals/problem-list persist across visits.
/// </summary>
public static class PatientClinicalEndpoints
{
    public static IEndpointRouteBuilder MapPatientClinicalEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/patients/{patientId:guid}");

        group.MapPost("/vitals", RecordVitalsAsync)
            .RequireAuthorization(Permissions.Clinical.Consult);

        group.MapGet("/vitals", GetVitalsAsync)
            .RequireAuthorization(Permissions.Clinical.View);

        group.MapPost("/immunizations", RecordImmunizationAsync)
            .RequireAuthorization(Permissions.Clinical.Consult);

        group.MapGet("/immunizations", GetImmunizationsAsync)
            .RequireAuthorization(Permissions.Clinical.View);

        group.MapPost("/conditions", AddConditionAsync)
            .RequireAuthorization(Permissions.Clinical.Consult);

        group.MapGet("/conditions", GetConditionsAsync)
            .RequireAuthorization(Permissions.Clinical.View);

        app.MapPost("/api/v1/conditions/{conditionId:guid}/resolve", ResolveConditionAsync)
            .RequireAuthorization(Permissions.Clinical.Consult);

        return app;
    }

    private static async Task<IResult> RecordVitalsAsync(
        Guid patientId, RecordVitalsRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new RecordVitalsCommand(
            patientId, request.TemperatureCelsius, request.SystolicBp, request.DiastolicBp,
            request.PulseRate, request.RespiratoryRate, request.OxygenSaturation,
            request.WeightKg, request.HeightCm), ct);
        return result.IsSuccess
            ? Results.Created($"/api/v1/patients/{patientId}/vitals/{result.Value.Id}", result.Value)
            : MapError(result.Error);
    }

    private static async Task<IResult> GetVitalsAsync(Guid patientId, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetVitalsQuery(patientId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> RecordImmunizationAsync(
        Guid patientId, RecordImmunizationRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new RecordImmunizationCommand(
            patientId, request.VaccineName, request.DoseNumber, request.AdministeredDate,
            request.NextDueDate, request.LotNumber, request.Site, request.Notes), ct);
        return result.IsSuccess
            ? Results.Created($"/api/v1/patients/{patientId}/immunizations/{result.Value.Id}", result.Value)
            : MapError(result.Error);
    }

    private static async Task<IResult> GetImmunizationsAsync(Guid patientId, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetImmunizationsQuery(patientId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> AddConditionAsync(
        Guid patientId, AddConditionRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new AddConditionCommand(
            patientId, request.Code, request.Description, request.OnsetDate), ct);
        return result.IsSuccess
            ? Results.Created($"/api/v1/patients/{patientId}/conditions/{result.Value.Id}", result.Value)
            : MapError(result.Error);
    }

    private static async Task<IResult> GetConditionsAsync(Guid patientId, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetConditionsQuery(patientId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> ResolveConditionAsync(
        Guid conditionId, ResolveConditionRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ResolveConditionCommand(conditionId, request.ResolvedDate), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static IResult MapError(Error error) => error.Code switch
    {
        ErrorCodes.NotFound => Results.NotFound(new { error = error.Message }),
        ErrorCodes.InvalidOperation => Results.BadRequest(new { error = error.Message }),
        ErrorCodes.Conflict => Results.Conflict(new { error = error.Message }),
        ErrorCodes.Validation => Results.BadRequest(new { error = error.Message }),
        ErrorCodes.Forbidden => Results.Forbid(),
        ErrorCodes.Unauthorized => Results.Unauthorized(),
        _ => Results.BadRequest(new { error = error.Message })
    };
}
