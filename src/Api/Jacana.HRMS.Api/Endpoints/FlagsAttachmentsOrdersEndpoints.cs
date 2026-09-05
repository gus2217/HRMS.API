using Jacana.Clinical.Application.Abstractions;
using Jacana.Clinical.Application.DTOs;
using Jacana.Clinical.Application.Features.PatientClinical;
using Jacana.Clinical.Domain;
using Jacana.Identity.Application;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Jacana.HRMS.Api.Endpoints;

/// <summary>
/// Patient flags, attachments and diagnostic (imaging/procedure) orders.
/// Reuses the existing Clinical.View / Clinical.Consult permissions — flags,
/// attachments and orders are clinical artefacts gated the same way as the
/// consultation record.
/// </summary>
public static class FlagsAttachmentsOrdersEndpoints
{
    public static IEndpointRouteBuilder MapFlagsAttachmentsOrdersEndpoints(this IEndpointRouteBuilder app)
    {
        var patient = app.MapGroup("/api/v1/patients/{patientId:guid}");
        var orders = app.MapGroup("/api/v1/diagnostic-orders");

        // ── Flags ─────────────────────────────────────────────────────────────
        patient.MapGet("/flags", GetActiveFlagsAsync)
            .RequireAuthorization(Permissions.Clinical.View);
        patient.MapGet("/flags/all", GetAllFlagsAsync)
            .RequireAuthorization(Permissions.Clinical.View);
        patient.MapPost("/flags", RaiseFlagAsync)
            .RequireAuthorization(Permissions.Clinical.Consult);
        app.MapPost("/api/v1/flags/{flagId:guid}/deactivate", DeactivateFlagAsync)
            .RequireAuthorization(Permissions.Clinical.Consult);

        // ── Attachments ───────────────────────────────────────────────────────
        patient.MapGet("/attachments", GetAttachmentsAsync)
            .RequireAuthorization(Permissions.Clinical.View);
        patient.MapPost("/attachments", UploadAttachmentAsync)
            .RequireAuthorization(Permissions.Clinical.Consult)
            .DisableAntiforgery();
        app.MapGet("/api/v1/attachments/{attachmentId:guid}/download", DownloadAttachmentAsync)
            .RequireAuthorization(Permissions.Clinical.View);
        app.MapDelete("/api/v1/attachments/{attachmentId:guid}", DeleteAttachmentAsync)
            .RequireAuthorization(Permissions.Clinical.Consult);

        // ── Diagnostic orders ─────────────────────────────────────────────────
        orders.MapPost("/", CreateOrderAsync)
            .RequireAuthorization(Permissions.Clinical.Consult);
        orders.MapPost("/{orderId:guid}/schedule", ScheduleOrderAsync)
            .RequireAuthorization(Permissions.Clinical.Consult);
        orders.MapPost("/{orderId:guid}/perform", PerformOrderAsync)
            .RequireAuthorization(Permissions.Clinical.Consult);
        orders.MapPost("/{orderId:guid}/report", ReportOrderAsync)
            .RequireAuthorization(Permissions.Clinical.Consult);
        orders.MapPost("/{orderId:guid}/cancel", CancelOrderAsync)
            .RequireAuthorization(Permissions.Clinical.Consult);
        patient.MapGet("/diagnostic-orders", GetOrdersByPatientAsync)
            .RequireAuthorization(Permissions.Clinical.View);
        app.MapGet("/api/v1/consultations/{consultationId:guid}/diagnostic-orders", GetOrdersByConsultationAsync)
            .RequireAuthorization(Permissions.Clinical.View);

        return app;
    }

    // ── Flags ─────────────────────────────────────────────────────────────────
    private static async Task<IResult> GetActiveFlagsAsync(Guid patientId, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetActiveFlagsQuery(patientId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> GetAllFlagsAsync(Guid patientId, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetAllFlagsQuery(patientId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> RaiseFlagAsync(
        Guid patientId, RaisePatientFlagRequestDto request, ISender sender, CancellationToken ct)
    {
        if (!Enum.TryParse<PatientFlagType>(request.Type, ignoreCase: true, out var type))
            return Results.BadRequest(new { error = "Flag type is invalid." });

        var result = await sender.Send(new RaisePatientFlagCommand(patientId, type, request.Message), ct);
        return result.IsSuccess ? Results.Created($"/api/v1/patients/{patientId}/flags/{result.Value.Id}", result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> DeactivateFlagAsync(Guid flagId, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new DeactivatePatientFlagCommand(flagId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    // ── Attachments ───────────────────────────────────────────────────────────
    private static async Task<IResult> GetAttachmentsAsync(Guid patientId, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetAttachmentsQuery(patientId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> UploadAttachmentAsync(
        Guid patientId, [FromForm] IFormFile file, [FromForm] string? category, ISender sender, CancellationToken ct)
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

        var result = await sender.Send(new UploadAttachmentCommand(
            patientId, file.FileName, file.ContentType, category ?? "General", content), ct);
        return result.IsSuccess ? Results.Created($"/api/v1/patients/{patientId}/attachments/{result.Value.Id}", result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> DownloadAttachmentAsync(
        Guid attachmentId, ISender sender, IPatientClinicalRepository repository, IFileStorage fileStorage, CancellationToken ct)
    {
        var attachment = await repository.GetAttachmentAsync(attachmentId, ct);
        if (attachment is null) return Results.NotFound(new { error = "Attachment not found." });

        var bytes = await fileStorage.ReadAsync(attachment.StorageKey, ct);
        if (bytes is null) return Results.NotFound(new { error = "Attachment content is missing." });

        return Results.File(bytes, attachment.ContentType, attachment.FileName);
    }

    private static async Task<IResult> DeleteAttachmentAsync(Guid attachmentId, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteAttachmentCommand(attachmentId), ct);
        return result.IsSuccess ? Results.NoContent() : MapError(result.Error);
    }

    // ── Diagnostic orders ─────────────────────────────────────────────────────
    private static async Task<IResult> CreateOrderAsync(
        CreateDiagnosticOrderRequestDto request, ISender sender, CancellationToken ct)
    {
        if (!Enum.TryParse<DiagnosticOrderType>(request.Type, ignoreCase: true, out var type))
            return Results.BadRequest(new { error = "Order type is invalid." });
        if (!Enum.TryParse<DiagnosticOrderPriority>(request.Priority, ignoreCase: true, out var priority))
            return Results.BadRequest(new { error = "Order priority is invalid." });

        var result = await sender.Send(new CreateDiagnosticOrderCommand(
            request.PatientId, request.ConsultationId, type, request.Name,
            request.BodySite, request.ClinicalIndication, priority), ct);
        return result.IsSuccess ? Results.Created($"/api/v1/diagnostic-orders/{result.Value.Id}", result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> ScheduleOrderAsync(Guid orderId, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ScheduleDiagnosticOrderCommand(orderId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> PerformOrderAsync(Guid orderId, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new PerformDiagnosticOrderCommand(orderId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> ReportOrderAsync(
        Guid orderId, ReportDiagnosticOrderRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ReportDiagnosticOrderCommand(orderId, request.Report), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> CancelOrderAsync(
        Guid orderId, CancelDiagnosticOrderRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new CancelDiagnosticOrderCommand(orderId, request.Reason), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> GetOrdersByPatientAsync(Guid patientId, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetDiagnosticOrdersByPatientQuery(patientId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> GetOrdersByConsultationAsync(Guid consultationId, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetDiagnosticOrdersByConsultationQuery(consultationId), ct);
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
