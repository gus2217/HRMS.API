using Jacana.Billing.Application.DTOs;
using Jacana.Billing.Application.Features.Billing;
using Jacana.Identity.Application;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.HRMS.Api.Endpoints;

/// <summary>Billing endpoints: bind → dispatch → map result → return.</summary>
public static class BillingEndpoints
{
    public static IEndpointRouteBuilder MapBillingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/billing");

        group.MapPost("/invoices", IssueInvoiceAsync)
            .RequireAuthorization(Permissions.Billing.IssueInvoice);

        group.MapGet("/invoices", SearchInvoicesAsync)
            .RequireAuthorization(Permissions.Billing.View);

        group.MapGet("/invoices/{id:guid}", GetInvoiceAsync)
            .RequireAuthorization(Permissions.Billing.View);

        group.MapPost("/invoices/{id:guid}/cancel", CancelInvoiceAsync)
            .RequireAuthorization(Permissions.Billing.IssueInvoice);

        group.MapPost("/payments", RecordPaymentAsync)
            .RequireAuthorization(Permissions.Billing.RecordPayment);

        group.MapPost("/mpesa/callback", MPesaCallbackAsync)
            .AllowAnonymous();

        group.MapPost("/sha/claims", SubmitShaClaimAsync)
            .RequireAuthorization(Permissions.Billing.IssueInvoice);

        group.MapPost("/sha/callback", ShaCallbackAsync)
            .AllowAnonymous();

        return app;
    }

    private static async Task<IResult> IssueInvoiceAsync(
        IssueInvoiceRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new IssueInvoiceCommand(
            request.PatientId, request.ConsultationId, request.PrimaryPaymentMethod,
            request.Lines.Select(l => new InvoiceLineInput(l.ServiceCode, l.Description, l.Quantity, l.UnitPrice)).ToArray()), ct);
        return result.IsSuccess ? Results.Created($"/api/v1/billing/invoices/{result.Value.Id}", result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> SearchInvoicesAsync(
        ISender sender, CancellationToken ct, string? status = null, Guid? consultationId = null, int pageNumber = 1, int pageSize = 20)
    {
        var result = await sender.Send(new SearchInvoicesQuery(pageNumber, pageSize, status, consultationId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> GetInvoiceAsync(Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetInvoiceQuery(id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> CancelInvoiceAsync(Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new CancelInvoiceCommand(id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> RecordPaymentAsync(
        RecordPaymentRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new RecordPaymentCommand(
            request.InvoiceId, request.AmountPaid, request.Method, request.ProviderTransactionReference), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> MPesaCallbackAsync(
        MPesaCallbackDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new RecordMPesaCallbackCommand(request), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> SubmitShaClaimAsync(
        SubmitShaClaimRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new SubmitShaClaimCommand(
            request.InvoiceId, request.ShaClaimReference), ct);
        return result.IsSuccess ? Results.Created($"/api/v1/billing/sha/claims/{result.Value.ShaClaimId}", result.Value) : MapError(result.Error);
    }

    private static async Task<IResult> ShaCallbackAsync(
        ShaCallbackRequestDto request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new RecordShaCallbackCommand(
            request.ShaClaimReference, request.Status, request.RejectionReason), ct);
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
