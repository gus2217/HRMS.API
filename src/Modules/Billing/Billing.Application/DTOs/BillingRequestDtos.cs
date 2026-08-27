using Jacana.Billing.Domain;

namespace Jacana.Billing.Application.DTOs;

// HTTP request bindings for billing endpoints.

public sealed record IssueInvoiceRequestDto(
    Guid PatientId,
    Guid? ConsultationId,
    PaymentMethod? PrimaryPaymentMethod,
    IReadOnlyList<InvoiceLineRequestDto> Lines);

public sealed record InvoiceLineRequestDto(string ServiceCode, string Description, int Quantity, decimal UnitPrice);

public sealed record RecordPaymentRequestDto(
    Guid InvoiceId,
    decimal AmountPaid,
    PaymentMethod Method,
    string ProviderTransactionReference);

public sealed record SubmitShaClaimRequestDto(Guid InvoiceId, string ShaClaimReference);

public sealed record ShaCallbackRequestDto(
    string ShaClaimReference,
    ShaClaimStatus Status,
    string? RejectionReason);
