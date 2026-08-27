using Jacana.SharedKernel.Domain;

namespace Jacana.Billing.Domain;

/// <summary>A Social Health Authority (SHA) claim against an invoice.</summary>
public sealed class ShaClaim : AggregateRoot<Guid>
{
    private ShaClaim() { } // EF

    private ShaClaim(Guid id, FacilityId facilityId, Guid invoiceId, string reference,
        ShaClaimStatus status, DateTime submittedAtUtc)
        : base(id)
    {
        FacilityId = facilityId;
        InvoiceId = invoiceId;
        ShaClaimReference = reference;
        Status = status;
        SubmittedAtUtc = submittedAtUtc;
    }

    public FacilityId FacilityId { get; private set; } = null!;
    public Guid InvoiceId { get; private set; }
    public string ShaClaimReference { get; private set; } = string.Empty;
    public ShaClaimStatus Status { get; private set; }
    public string? RejectionReason { get; private set; }
    public DateTime SubmittedAtUtc { get; private set; }

    public static Result<ShaClaim> Submit(
        Guid id, FacilityId facilityId, Guid invoiceId, string reference, DateTime submittedAtUtc)
    {
        if (invoiceId == Guid.Empty) return Error.Validation("Invoice is required.");
        if (string.IsNullOrWhiteSpace(reference)) return Error.Validation("SHA claim reference is required.");
        return new ShaClaim(id, facilityId, invoiceId, reference.Trim(),
            ShaClaimStatus.Submitted, submittedAtUtc);
    }

    public Result Reject(string reason)
    {
        if (Status is ShaClaimStatus.Paid or ShaClaimStatus.Rejected)
            return Error.InvalidOperation($"Cannot reject a claim in status {Status}.");
        Status = ShaClaimStatus.Rejected;
        RejectionReason = reason;
        return Result.Success();
    }

    public Result Approve() { Status = ShaClaimStatus.Approved; return Result.Success(); }
    public Result MarkPaid() { Status = ShaClaimStatus.Paid; return Result.Success(); }
}
