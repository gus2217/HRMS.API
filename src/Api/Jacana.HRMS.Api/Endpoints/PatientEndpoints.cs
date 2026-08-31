using Jacana.Identity.Application;
using Jacana.PatientRegistration.Application.Features.Patients;
using Jacana.PatientRegistration.Application.DTOs;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.HRMS.Api.Endpoints;

/// <summary>
/// Patient Registration endpoints: bind → dispatch → map result → return.
/// No business logic lives here.
/// </summary>
public static class PatientEndpoints
{
    public static IEndpointRouteBuilder MapPatientEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/patients");

        group.MapPost("/", RegisterAsync)
            .RequireAuthorization(Permissions.Patients.Register);

        group.MapGet("/{id:guid}", GetAsync)
            .RequireAuthorization(Permissions.Patients.View);

        group.MapGet("/", SearchAsync)
            .RequireAuthorization(Permissions.Patients.View);

        group.MapPut("/{id:guid}/demographics", UpdateDemographicsAsync)
            .RequireAuthorization(Permissions.Patients.Update);

        group.MapPost("/{id:guid}/allergies", RegisterAllergyAsync)
            .RequireAuthorization(Permissions.Patients.Update);

        group.MapPost("/{id:guid}/consents", RecordConsentAsync)
            .RequireAuthorization(Permissions.Patients.Update);

        return app;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterPatientRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new RegisterPatientCommand(
            request.FirstName, request.LastName, request.DateOfBirth,
            request.Gender, request.Phone,
            request.NationalId, request.InsuranceType, request.InsuranceNumber, request.ClinicType,
            request.County, request.SubCounty, request.Ward, request.Line1), ct);

        if (result.IsFailure) return MapError(result.Error);

        // Duplicate candidates present → 409 with candidates for staff confirmation.
        if (result.Value.DuplicateCandidates.Count > 0)
            return Results.Conflict(new { duplicateCandidates = result.Value.DuplicateCandidates });

        return Results.Created($"/api/v1/patients/{result.Value.Id}", result.Value);
    }

    private static async Task<IResult> GetAsync(Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetPatientQuery(id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> SearchAsync(
        string? search, ISender sender, CancellationToken ct, string? sort = null, int pageNumber = 1, int pageSize = 50)
    {
        var result = await sender.Send(new SearchPatientsQuery(search, pageNumber, pageSize, sort), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> UpdateDemographicsAsync(
        Guid id, UpdatePatientDemographicsRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new UpdatePatientDemographicsCommand(
            id, request.FirstName, request.LastName, request.DateOfBirth,
            request.Gender, request.MaritalStatus, request.Phone,
            request.County, request.SubCounty, request.Ward, request.Line1), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> RegisterAllergyAsync(
        Guid id, RegisterAllergyRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new RegisterAllergyCommand(
            id, request.Substance, request.Severity, request.Notes), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> RecordConsentAsync(
        Guid id, RecordConsentRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new RecordConsentCommand(id, request.Type, request.Granted), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static IResult MapError(Error error) => error.Code switch
    {
        ErrorCodes.NotFound => Results.NotFound(new { error = error.Message }),
        ErrorCodes.Conflict => Results.Conflict(new { error = error.Message }),
        ErrorCodes.Validation => Results.BadRequest(new { error = error.Message }),
        ErrorCodes.Forbidden => Results.Forbid(),
        ErrorCodes.Unauthorized => Results.Unauthorized(),
        _ => Results.BadRequest(new { error = error.Message })
    };
}
