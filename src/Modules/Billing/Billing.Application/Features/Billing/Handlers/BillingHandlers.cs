using Jacana.Billing.Application.Abstractions;
using Jacana.Billing.Application.DTOs;
using Jacana.Billing.Domain;
using Jacana.SharedKernel.Application.Abstractions;
using Jacana.SharedKernel.Application.Common;
using Jacana.SharedKernel.Domain;
using MediatR;

namespace Jacana.Billing.Application.Features.Billing.Handlers;

public sealed class IssueInvoiceCommandHandler(
    IInvoiceRepository invoices,
    ICurrentUser currentUser)
    : IRequestHandler<IssueInvoiceCommand, Result<InvoiceDetailDto>>
{
    public async Task<Result<InvoiceDetailDto>> Handle(IssueInvoiceCommand request, CancellationToken ct)
    {
        var invoice = Invoice.Create(Guid.NewGuid(), currentUser.FacilityId,
            request.PatientId, request.ConsultationId);
        if (invoice.IsFailure) return invoice.Error;

        foreach (var line in request.Lines)
        {
            var price = Money.Create(line.UnitPrice);
            if (price.IsFailure) return price.Error;
            var add = invoice.Value.AddLine(line.ServiceCode, line.Description, line.Quantity, price.Value);
            if (add.IsFailure) return add.Error;
        }

        var issue = invoice.Value.Issue(request.PrimaryPaymentMethod);
        if (issue.IsFailure) return issue.Error;

        await invoices.AddAsync(invoice.Value, ct);
        // Map from the in-memory aggregate — the unit-of-work transaction has not
        // committed yet, so a re-query would not see the new row.
        return InvoiceMapper.ToDetail(invoice.Value);
    }
}

public sealed class RecordPaymentCommandHandler(
    IPaymentRepository payments,
    IInvoiceRepository invoices,
    ICurrentUser currentUser,
    IClock clock)
    : IRequestHandler<RecordPaymentCommand, Result<PaymentReceiptDto>>
{
    public async Task<Result<PaymentReceiptDto>> Handle(RecordPaymentCommand request, CancellationToken ct)
    {
        // Idempotency: a duplicate provider reference is a no-op success, never a duplicate row.
        var existing = await payments.GetByProviderReferenceAsync(request.ProviderTransactionReference, ct);
        if (existing is not null)
            return new PaymentReceiptDto(existing.Id, existing.InvoiceId, existing.AmountPaid.Amount,
                existing.Method.ToString(), existing.ProviderTransactionReference, existing.Status.ToString());

        var invoice = await invoices.GetByIdAsync(request.InvoiceId, ct);
        if (invoice is null) return Error.NotFound("Invoice not found.");

        var amount = Money.Create(request.AmountPaid);
        if (amount.IsFailure) return amount.Error;

        var record = Payment.Create(
            Guid.NewGuid(), currentUser.FacilityId, request.InvoiceId, amount.Value,
            request.Method, request.ProviderTransactionReference, PaymentStatus.Confirmed, clock.UtcNow);
        if (record.IsFailure) return record.Error;

        var apply = invoice.RecordPayment(amount.Value);
        if (apply.IsFailure) return apply.Error;

        await payments.AddAsync(record.Value, ct);
        await invoices.UpdateAsync(invoice, ct);

        return new PaymentReceiptDto(record.Value.Id, record.Value.InvoiceId, record.Value.AmountPaid.Amount,
            record.Value.Method.ToString(), record.Value.ProviderTransactionReference, record.Value.Status.ToString());
    }
}

public sealed class RecordMPesaCallbackCommandHandler(
    IPaymentRepository payments,
    IInvoiceRepository invoices,
    ICurrentUser currentUser,
    IClock clock)
    : IRequestHandler<RecordMPesaCallbackCommand, Result<PaymentReceiptDto>>
{
    public async Task<Result<PaymentReceiptDto>> Handle(RecordMPesaCallbackCommand request, CancellationToken ct)
    {
        var stk = request.Callback?.Body?.StkCallback
            ?? throw new InvalidOperationException("M-Pesa callback payload is malformed.");

        // Idempotency key = CheckoutRequestID (Daraja's transaction id).
        var existing = await payments.GetByProviderReferenceAsync(stk.CheckoutRequestID, ct);
        if (existing is not null)
            return new PaymentReceiptDto(existing.Id, existing.InvoiceId, existing.AmountPaid.Amount,
                existing.Method.ToString(), existing.ProviderTransactionReference, existing.Status.ToString());

        if (stk.ResultCode != 0)
            return Error.InvalidOperation($"M-Pesa payment failed: {stk.ResultDesc}");

        // Extract amount + M-Pesa receipt from metadata.
        var metadata = stk.CallbackMetadata?.Item ?? [];
        decimal amount = 0m;
        string? mpesaRef = null;
        foreach (var item in metadata)
        {
            if (item.Name == "Amount") decimal.TryParse(item.Value?.ToString(), out amount);
            if (item.Name == "MpesaReceiptNumber") mpesaRef = item.Value?.ToString();
        }

        if (amount <= 0)
            return Error.InvalidOperation("M-Pesa callback did not include an amount.");

        // Find the invoice by a mapping of CheckoutRequestID -> invoice. In this slice, the
        // CheckoutRequestID is stored as the provider reference on the pending Payment.
        var money = Money.Create(amount);
        if (money.IsFailure) return money.Error;

        var record = Payment.Create(
            Guid.NewGuid(), currentUser.FacilityId, Guid.Empty, money.Value,
            PaymentMethod.MPesa, stk.CheckoutRequestID, PaymentStatus.Confirmed, clock.UtcNow);
        if (record.IsFailure) return record.Error;

        await payments.AddAsync(record.Value, ct);

        return new PaymentReceiptDto(record.Value.Id, record.Value.InvoiceId, record.Value.AmountPaid.Amount,
            record.Value.Method.ToString(), record.Value.ProviderTransactionReference, record.Value.Status.ToString());
    }
}

public sealed class SubmitShaClaimCommandHandler(
    IShaClaimRepository claims,
    IInvoiceRepository invoices,
    ICurrentUser currentUser,
    IClock clock)
    : IRequestHandler<SubmitShaClaimCommand, Result<ShaClaimSubmissionDto>>
{
    public async Task<Result<ShaClaimSubmissionDto>> Handle(SubmitShaClaimCommand request, CancellationToken ct)
    {
        var invoice = await invoices.GetByIdAsync(request.InvoiceId, ct);
        if (invoice is null) return Error.NotFound("Invoice not found.");

        var claim = ShaClaim.Submit(Guid.NewGuid(), currentUser.FacilityId,
            request.InvoiceId, request.ShaClaimReference, clock.UtcNow);
        if (claim.IsFailure) return claim.Error;

        await claims.AddAsync(claim.Value, ct);

        return new ShaClaimSubmissionDto(claim.Value.Id, claim.Value.InvoiceId,
            claim.Value.ShaClaimReference, claim.Value.Status.ToString());
    }
}

public sealed class RecordShaCallbackCommandHandler(
    IShaClaimRepository claims)
    : IRequestHandler<RecordShaCallbackCommand, Result<ShaClaimSubmissionDto>>
{
    public async Task<Result<ShaClaimSubmissionDto>> Handle(RecordShaCallbackCommand request, CancellationToken ct)
    {
        var claim = await claims.GetByReferenceAsync(request.ShaClaimReference, ct);
        if (claim is null) return Error.NotFound("SHA claim not found.");

        Result result = request.Status switch
        {
            ShaClaimStatus.Rejected => claim.Reject(request.RejectionReason ?? "Rejected by SHA."),
            ShaClaimStatus.Approved => claim.Approve(),
            ShaClaimStatus.Paid => claim.MarkPaid(),
            _ => Result.Success()
        };
        if (result.IsFailure) return result.Error;

        await claims.UpdateAsync(claim, ct);

        return new ShaClaimSubmissionDto(claim.Id, claim.InvoiceId,
            claim.ShaClaimReference, claim.Status.ToString());
    }
}

public sealed class GetInvoiceQueryHandler(IInvoiceRepository invoices)
    : IRequestHandler<GetInvoiceQuery, Result<InvoiceDetailDto>>
{
    public async Task<Result<InvoiceDetailDto>> Handle(GetInvoiceQuery request, CancellationToken ct)
    {
        var detail = await invoices.GetDetailAsync(request.InvoiceId, ct);
        return detail is null ? Error.NotFound("Invoice not found.") : detail;
    }
}

public sealed class SearchInvoicesQueryHandler(
    IInvoiceRepository invoices,
    IPatientIdentityLookup patients)
    : IRequestHandler<SearchInvoicesQuery, Result<PagedResult<InvoiceListItemDto>>>
{
    public async Task<Result<PagedResult<InvoiceListItemDto>>> Handle(
        SearchInvoicesQuery request, CancellationToken ct)
    {
        var items = await invoices.SearchAsync(
            request.Status, request.PageNumber, request.PageSize, ct);
        var total = await invoices.CountAsync(request.Status, ct);

        var identities = await patients.GetIdentitiesAsync(
            items.Select(i => i.PatientId).ToArray(), ct);

        var rows = items.Select(i =>
        {
            identities.TryGetValue(i.PatientId, out var patient);
            return new InvoiceListItemDto(
                i.Id, i.PatientId,
                patient?.PatientNumber ?? string.Empty,
                patient?.FullName ?? string.Empty,
                i.Status, i.TotalAmount, i.CreatedAtUtc);
        }).ToArray();

        return Result.Success(new PagedResult<InvoiceListItemDto>(
            rows, total, request.PageNumber, request.PageSize));
    }
}
