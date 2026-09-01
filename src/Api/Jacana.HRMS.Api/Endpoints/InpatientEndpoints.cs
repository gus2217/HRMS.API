using Jacana.Identity.Application;
using Jacana.Inpatient.Application.Abstractions;
using Jacana.Inpatient.Application.Features.Inpatient;
using Jacana.Inpatient.Application.DTOs;
using Jacana.Inpatient.Domain;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.HRMS.Api.Endpoints;

/// <summary>
/// Inpatient endpoints: wards (admin-managed), admissions (hospital-oriented,
/// ward-linked, with admitting diagnosis + attending clinician), day-to-day ward
/// medical records (SOAP + vitals + media uploads) and discharge gating.
/// </summary>
public static class InpatientEndpoints
{
    public static IEndpointRouteBuilder MapInpatientEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/inpatient");

        // ── Wards (admin) ───────────────────────────────────────────────────
        group.MapGet("/wards", GetWardsAsync)
            .RequireAuthorization(Permissions.Clinical.View);

        group.MapPost("/wards", CreateWardAsync)
            .RequireAuthorization(Permissions.Users.View);

        group.MapPut("/wards/{id:guid}", UpdateWardAsync)
            .RequireAuthorization(Permissions.Users.View);

        group.MapPost("/wards/{id:guid}/deactivate", DeactivateWardAsync)
            .RequireAuthorization(Permissions.Users.View);

        group.MapPost("/wards/{id:guid}/reactivate", ReactivateWardAsync)
            .RequireAuthorization(Permissions.Users.View);

        // ── Admissions ──────────────────────────────────────────────────────
        group.MapPost("/admissions", AdmitAsync)
            .RequireAuthorization(Permissions.Clinical.Consult);

        group.MapGet("/admissions", SearchAdmissionsAsync)
            .RequireAuthorization(Permissions.Clinical.View);

        group.MapGet("/admissions/{id:guid}", GetAdmissionAsync)
            .RequireAuthorization(Permissions.Clinical.View);

        group.MapPost("/admissions/{id:guid}/discharge", DischargeAsync)
            .RequireAuthorization(Permissions.Clinical.Consult);

        group.MapPost("/admissions/{id:guid}/transfer", TransferAsync)
            .RequireAuthorization(Permissions.Clinical.Consult);

        group.MapPost("/admissions/{id:guid}/notes", AddWardNoteAsync)
            .RequireAuthorization(Permissions.Clinical.Consult);

        // ── Day-to-day ward medical records (SOAP + vitals) ─────────────────
        group.MapPost("/admissions/{id:guid}/medical-records", AddMedicalRecordAsync)
            .RequireAuthorization(Permissions.Clinical.Consult);

        group.MapPost("/medical-records/{recordId:guid}/attachments", AttachFileAsync)
            .RequireAuthorization(Permissions.Clinical.Consult)
            .DisableAntiforgery();

        group.MapGet("/medical-records/{recordId:guid}/attachments/{attachmentId:guid}/download", DownloadFileAsync)
            .RequireAuthorization(Permissions.Clinical.View);

        group.MapGet("/ward-occupancy", GetWardOccupancyAsync)
            .RequireAuthorization(Permissions.Clinical.View);

        return app;
    }

    // ── Wards ────────────────────────────────────────────────────────────────

    private static async Task<IResult> GetWardsAsync(
        ISender sender, CancellationToken ct, bool activeOnly = false)
    {
        var result = await sender.Send(new GetWardsQuery(activeOnly), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> CreateWardAsync(
        CreateWardRequestDto request, ISender sender, CancellationToken ct)
    {
        if (!Enum.TryParse<WardType>(request.Type, ignoreCase: true, out var type))
            return Results.BadRequest(new { error = "Ward type is invalid." });

        var result = await sender.Send(new CreateWardCommand(request.Name, type, request.TotalBeds), ct);
        return result.IsSuccess ? Results.Created($"/api/v1/inpatient/wards/{result.Value.Id}", result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> UpdateWardAsync(
        Guid id, UpdateWardRequestDto request, ISender sender, CancellationToken ct)
    {
        if (!Enum.TryParse<WardType>(request.Type, ignoreCase: true, out var type))
            return Results.BadRequest(new { error = "Ward type is invalid." });

        var result = await sender.Send(new UpdateWardCommand(id, request.Name, type, request.TotalBeds), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> DeactivateWardAsync(Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new DeactivateWardCommand(id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> ReactivateWardAsync(Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ReactivateWardCommand(id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    // ── Admissions ───────────────────────────────────────────────────────────

    private static async Task<IResult> AdmitAsync(
        AdmitPatientRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new AdmitPatientCommand(
            request.PatientId, request.AdmittingClinicianUserId, request.WardId,
            request.BedNumber, request.AdmittingDiagnosis, request.AttendingClinicianUserId), ct);
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

    private static async Task<IResult> TransferAsync(
        Guid id, TransferPatientRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new TransferPatientCommand(
            id, request.TargetWardId, request.BedNumber), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> AddWardNoteAsync(
        Guid id, AddWardNoteRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new AddWardNoteCommand(id, request.Content), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    // ── Medical records + media ──────────────────────────────────────────────

    private static async Task<IResult> AddMedicalRecordAsync(
        Guid id, AddMedicalRecordRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new AddMedicalRecordCommand(
            id, request.TemperatureCelsius, request.SystolicBp, request.DiastolicBp,
            request.PulseRate, request.RespiratoryRate, request.OxygenSaturation,
            request.WeightKg, request.Subjective, request.Objective,
            request.Assessment, request.Plan), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> AttachFileAsync(
        Guid recordId, IFormFile file, ISender sender, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return Results.BadRequest(new { error = "A file is required." });
        if (file.Length > 20 * 1024 * 1024)
            return Results.BadRequest(new { error = "File exceeds the 20 MB limit." });

        byte[] content;
        using (var ms = new MemoryStream())
        {
            await file.CopyToAsync(ms, ct);
            content = ms.ToArray();
        }

        var result = await sender.Send(new AttachMedicalRecordFileCommand(
            recordId, file.FileName, file.ContentType, content), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> DownloadFileAsync(
        Guid recordId, Guid attachmentId, ISender sender, IAdmissionRepository repository,
        IFileStorage fileStorage, CancellationToken ct)
    {
        var attachment = await repository.GetAttachmentAsync(attachmentId, ct);
        if (attachment is null) return Results.NotFound(new { error = "Attachment not found." });

        var bytes = await fileStorage.ReadAsync(attachment.StorageKey, ct);
        if (bytes is null) return Results.NotFound(new { error = "Attachment content is missing." });

        return Results.File(bytes, attachment.ContentType, attachment.FileName);
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
