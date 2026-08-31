using Jacana.Identity.Application;
using Jacana.Pharmacy.Application.Features.Pharmacy;
using Jacana.Pharmacy.Application.DTOs;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.HRMS.Api.Endpoints;

/// <summary>Pharmacy endpoints: bind → dispatch → map result → return.</summary>
public static class PharmacyEndpoints
{
    public static IEndpointRouteBuilder MapPharmacyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/pharmacy");

        group.MapPost("/prescriptions", CreatePrescriptionAsync)
            .RequireAuthorization(Permissions.Clinical.RecordDiagnosis);

        group.MapGet("/prescriptions", SearchPrescriptionsAsync)
            .RequireAuthorization(Permissions.Pharmacy.Dispense);

        group.MapGet("/prescriptions/{id:guid}", GetPrescriptionAsync)
            .RequireAuthorization(Permissions.Pharmacy.Dispense);

        group.MapGet("/consultations/{consultationId:guid}/prescriptions", GetPrescriptionsByConsultationAsync)
            .RequireAuthorization(Permissions.Clinical.View);

        group.MapGet("/reservations", GetReservationsAsync)
            .RequireAuthorization(Permissions.Clinical.RecordDiagnosis);

        group.MapPost("/dispense", DispenseAsync)
            .RequireAuthorization(Permissions.Pharmacy.Dispense);

        return app;
    }

    private static async Task<IResult> CreatePrescriptionAsync(
        CreatePrescriptionRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new CreatePrescriptionCommand(
            request.PatientId, request.ConsultationId,
            request.Items.Select(i => new PrescriptionItemInput(
                i.DrugId, i.DosageInstructions, i.Route, i.Frequency, i.DurationDays, i.QuantityPrescribed)).ToArray()), ct);
        return result.IsSuccess ? Results.Created($"/api/v1/pharmacy/prescriptions/{result.Value.Id}", result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> SearchPrescriptionsAsync(
        ISender sender, CancellationToken ct, string? status = null, Guid? patientId = null, int pageNumber = 1, int pageSize = 20)
    {
        var result = await sender.Send(new SearchPrescriptionsQuery(pageNumber, pageSize, status, patientId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> GetPrescriptionAsync(Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetPrescriptionQuery(id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> GetPrescriptionsByConsultationAsync(
        Guid consultationId, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetPrescriptionsByConsultationQuery(consultationId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> GetReservationsAsync(ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetReservationsQuery(), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> DispenseAsync(
        DispenseMedicationRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new DispenseMedicationCommand(
            request.PrescriptionId, request.PrescriptionItemId, request.Quantity), ct);
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
