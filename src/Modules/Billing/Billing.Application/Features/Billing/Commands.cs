using Jacana.Billing.Application.DTOs;
using Jacana.Billing.Domain;
using Jacana.SharedKernel.Application;
using Jacana.SharedKernel.Domain;

namespace Jacana.Billing.Application.Features.Billing;

public sealed record InvoiceLineInput(string ServiceCode, string Description, int Quantity, decimal UnitPrice);

public sealed record IssueInvoiceCommand(
    Guid PatientId,
    Guid? ConsultationId,
    PaymentMethod? PrimaryPaymentMethod,
    IReadOnlyList<InvoiceLineInput> Lines)
    : ICommand<Result<InvoiceDetailDto>>;

public sealed record RecordPaymentCommand(
    Guid InvoiceId,
    decimal AmountPaid,
    PaymentMethod Method,
    string ProviderTransactionReference)
    : ICommand<Result<PaymentReceiptDto>>;

public sealed record RecordMPesaCallbackCommand(MPesaCallbackDto Callback)
    : ICommand<Result<PaymentReceiptDto>>;

public sealed record SubmitShaClaimCommand(
    Guid InvoiceId,
    string ShaClaimReference)
    : ICommand<Result<ShaClaimSubmissionDto>>;

public sealed record RecordShaCallbackCommand(
    string ShaClaimReference,
    ShaClaimStatus Status,
    string? RejectionReason)
    : ICommand<Result<ShaClaimSubmissionDto>>;
