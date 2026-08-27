using Jacana.SharedKernel.Domain;

namespace Jacana.Billing.Domain;

/// <summary>
/// A payment against an invoice. Own aggregate — payments are idempotent, keyed by the
/// provider transaction reference (unique index), and must not share a transaction
/// boundary with Invoice mutation (avoids deadlocks under concurrent callback delivery).
/// </summary>
public sealed class Payment : AggregateRoot<Guid>
{
    private Payment() { } // EF

    private Payment(Guid id, FacilityId facilityId, Guid invoiceId, Money amountPaid,
        PaymentMethod method, string providerTransactionReference, PaymentStatus status,
        DateTime receivedAtUtc)
        : base(id)
    {
        FacilityId = facilityId;
        InvoiceId = invoiceId;
        AmountPaid = amountPaid;
        Method = method;
        ProviderTransactionReference = providerTransactionReference;
        Status = status;
        ReceivedAtUtc = receivedAtUtc;
    }

    public FacilityId FacilityId { get; private set; } = null!;
    public Guid InvoiceId { get; private set; }
    public Money AmountPaid { get; private set; } = null!;
    public PaymentMethod Method { get; private set; }
    public string ProviderTransactionReference { get; private set; } = string.Empty;
    public PaymentStatus Status { get; private set; }
    public DateTime ReceivedAtUtc { get; private set; }

    public static Result<Payment> Create(
        Guid id, FacilityId facilityId, Guid invoiceId, Money amountPaid,
        PaymentMethod method, string providerTransactionReference, PaymentStatus status,
        DateTime receivedAtUtc)
    {
        if (invoiceId == Guid.Empty) return Error.Validation("Invoice is required.");
        if (amountPaid.Amount <= 0) return Error.Validation("Payment amount must be positive.");
        if (string.IsNullOrWhiteSpace(providerTransactionReference))
            return Error.Validation("Provider transaction reference is required.");
        return new Payment(id, facilityId, invoiceId, amountPaid, method,
            providerTransactionReference.Trim(), status, receivedAtUtc);
    }

    public Result Confirm() { Status = PaymentStatus.Confirmed; return Result.Success(); }
    public Result MarkFailed() { Status = PaymentStatus.Failed; return Result.Success(); }
    public Result Reverse() { Status = PaymentStatus.Reversed; return Result.Success(); }
}
