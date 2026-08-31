namespace Jacana.Billing.Application.DTOs;

public sealed record InvoiceLineDto(
    Guid Id,
    string ServiceCode,
    string Description,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    string Status);

public sealed record InvoiceSummaryDto(
    Guid Id,
    Guid PatientId,
    string Status,
    decimal TotalAmount,
    DateTime CreatedAtUtc);

/// <summary>List-view row with patient display identity resolved cross-schema.</summary>
public sealed record InvoiceListItemDto(
    Guid Id,
    Guid PatientId,
    string PatientNumber,
    string PatientName,
    string Status,
    decimal TotalAmount,
    DateTime CreatedAtUtc);

public sealed record InvoiceDetailDto(
    Guid Id,
    Guid PatientId,
    Guid? ConsultationId,
    string Status,
    decimal TotalAmount,
    string? PrimaryPaymentMethod,
    DateTime CreatedAtUtc,
    IReadOnlyList<InvoiceLineDto> Lines);

public sealed record PaymentReceiptDto(
    Guid PaymentId,
    Guid InvoiceId,
    decimal AmountPaid,
    string Method,
    string ProviderTransactionReference,
    string Status);

public sealed record ShaClaimSubmissionDto(
    Guid ShaClaimId,
    Guid InvoiceId,
    string ShaClaimReference,
    string Status);

/// <summary>
/// M-Pesa Daraja callback payload, modeled against Safaricom's published STK Push
/// callback contract. Field names are exact — do not rename.
/// </summary>
public sealed record MPesaCallbackDto(
    MPesaCallbackBodyDto Body);

public sealed record MPesaCallbackBodyDto(
    MPesaStkCallbackDto StkCallback);

public sealed record MPesaStkCallbackDto(
    string MerchantRequestID,
    string CheckoutRequestID,
    int ResultCode,
    string ResultDesc,
    MPesaCallbackMetadataDto? CallbackMetadata);

public sealed record MPesaCallbackMetadataDto(
    IReadOnlyList<MPesaCallbackItemDto> Item);

public sealed record MPesaCallbackItemDto(string Name, object? Value);
